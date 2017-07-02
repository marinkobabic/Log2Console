using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;

using Log2Console.Log;

namespace Log2Console.Receiver
{
    public class Log4NetParser : ParserBase
    {
        static readonly DateTime s1970 = new DateTime(1970, 1, 1);

        /// <summary>
        /// We can share parser context to improve performance
        /// </summary>
        private readonly XmlParserContext _xmlContext = CreateContext();

        private static XmlParserContext CreateContext()
        {
            var nt = new NameTable();
            var nsmanager = new XmlNamespaceManager(nt);
            nsmanager.AddNamespace("log4net", "http://csharptest.net/downloads/schema/log4net.xsd");
            return new XmlParserContext(nt, nsmanager, "elem", XmlSpace.None, Encoding.UTF8);
        }

        /// <summary>
        /// We can share settings to improve performance
        /// </summary>
        readonly XmlReaderSettings _xmlSettings = CreateSettings();

        static XmlReaderSettings CreateSettings()
        {
            return new XmlReaderSettings { CloseInput = false, ValidationType = ValidationType.None };
        }

        private bool CanRead(XmlReader reader)
        {
            reader.Read();
            return reader.MoveToContent() == XmlNodeType.Element && reader.Name == "log4net:event";
        }


        /// <summary>
        /// Here we expect the log event to use the log4net schema.
        /// Sample:
        ///     <log4net:event logger="Statyk7.Another.Name.DummyManager" timestamp="1184286222308" level="ERROR" thread="1">
        ///         <log4net:message>This is an Message</log4net:message>
        ///         <log4net:properties>
        ///             <log4net:data name="log4netmachinename" value="remserver" />
        ///             <log4net:data name="log4net:HostName" value="remserver" />
        ///             <log4net:data name="log4net:UserName" value="REMSERVER\Statyk7" />
        ///             <log4net:data name="log4netapp" value="Test.exe" />
        ///         </log4net:properties>
        ///     </log4net:event>
        /// </summary>
        /// 
        /// Implementation inspired from: http://geekswithblogs.net/kobush/archive/2006/04/20/75717.aspx
        /// 
        protected override void ParseInternal(Stream logStream, string defaultLogger, Action<LogMessage> logMsgAction )
        {
            using (XmlReader reader = XmlReader.Create(logStream, this._xmlSettings, _xmlContext))
            {


                var logMsg = new LogMessage();

                if (!CanRead(reader))
                    throw new Exception("The Log Event is not a valid log4net Xml block.");

                logMsg.LoggerName = reader.GetAttribute("logger");
                logMsg.Level = LogLevels.Instance[reader.GetAttribute("level")];
                logMsg.ThreadName = reader.GetAttribute("thread");

                long timeStamp;
                if (long.TryParse(reader.GetAttribute("timestamp"), out timeStamp))
                    logMsg.TimeStamp = s1970.AddMilliseconds(timeStamp).ToLocalTime();

                int eventDepth = reader.Depth;
                reader.Read();
                while (reader.Depth > eventDepth)
                {
                    if (reader.MoveToContent() == XmlNodeType.Element)
                    {
                        switch (reader.Name)
                        {
                            case "log4net:message":
                                logMsg.Message = reader.ReadString();
                                break;

                            case "log4net:throwable":
                                logMsg.Message += Environment.NewLine + reader.ReadString();
                                break;

                            case "log4net:locationInfo":
                                logMsg.CallSiteClass = reader.GetAttribute("class");
                                logMsg.CallSiteMethod = reader.GetAttribute("method");
                                logMsg.SourceFileName = reader.GetAttribute("file");
                                uint sourceFileLine;
                                if (uint.TryParse(reader.GetAttribute("line"), out sourceFileLine))
                                    logMsg.SourceFileLineNr = sourceFileLine;
                                break;
                            case "nlog:eventSequenceNumber":
                                ulong sequenceNumber;
                                if (ulong.TryParse(reader.ReadString(), out sequenceNumber))
                                    logMsg.SequenceNr = sequenceNumber;
                                break;
                            case "nlog:locationInfo":
                                break;

                            case "log4net:properties":
                                reader.Read();
                                while (reader.MoveToContent() == XmlNodeType.Element
                                       && reader.Name == "log4net:data")
                                {
                                    string name = reader.GetAttribute("name");
                                    string value = reader.GetAttribute("value");
                                    if (name != null && name.ToLower().Equals("exceptions"))
                                    {
                                        logMsg.ExceptionString = value;
                                    }
                                    else
                                    {
                                        logMsg.Properties[name] = value;
                                    }

                                    reader.Read();
                                }

                                break;
                        }
                    }
                    reader.Read();
                }

                 logMsgAction(logMsg);
            }
        }

        protected override bool CanParseInternal(Stream stream)
        {
            stream.Position = 0;
            var reader = XmlReader.Create(stream, _xmlSettings, _xmlContext);
            var canParse = this.CanRead(reader);
            stream.Position = 0;
            return canParse;
        }
    }
}
