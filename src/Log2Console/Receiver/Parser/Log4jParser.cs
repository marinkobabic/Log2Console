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
    public class Log4jParser : ParserBase
    {
        static readonly DateTime s1970 = new DateTime(1970, 1, 1);

        /// <summary>
        /// We can share settings to improve performance
        /// </summary>
        readonly XmlReaderSettings _xmlSettings = CreateSettings();

        static XmlReaderSettings CreateSettings()
        {
            return new XmlReaderSettings { CloseInput = false, ValidationType = ValidationType.None };
        }

        /// <summary>
        /// We can share parser context to improve performance
        /// </summary>
        private readonly XmlParserContext _xmlContext = CreateContext();

        private static XmlParserContext CreateContext()
        {
            var nt = new NameTable();
            var nsmanager = new XmlNamespaceManager(nt);
            nsmanager.AddNamespace("log4j", "http://jakarta.apache.org/log4j/");
            nsmanager.AddNamespace("nlog", "http://nlog-project.org");
            return new XmlParserContext(nt, nsmanager, "elem", XmlSpace.None, Encoding.UTF8);
        }

        private bool CanRead(XmlReader reader)
        {
            reader.Read();
            return reader.MoveToContent() == XmlNodeType.Element && reader.Name == "log4j:event";
        }


        /// <summary>
        /// Here we expect the log event to use the log4j schema.
        /// Sample:
        ///     <log4j:event logger="Statyk7.Another.Name.DummyManager" timestamp="1184286222308" level="ERROR" thread="1">
        ///         <log4j:message>This is an Message</log4j:message>
        ///         <log4j:properties>
        ///             <log4j:data name="log4jmachinename" value="remserver" />
        ///             <log4j:data name="log4net:HostName" value="remserver" />
        ///             <log4j:data name="log4net:UserName" value="REMSERVER\Statyk7" />
        ///             <log4j:data name="log4japp" value="Test.exe" />
        ///         </log4j:properties>
        ///     </log4j:event>
        /// </summary>
        /// 
        /// Implementation inspired from: http://geekswithblogs.net/kobush/archive/2006/04/20/75717.aspx
        /// 
        protected override void ParseInternal(Stream logStream, string defaultLogger, Action<LogMessage> logMsgAction )
        {
            using (XmlReader reader = XmlReader.Create(logStream, this._xmlSettings, this._xmlContext))
            {


                var logMsg = new LogMessage();

                if (!CanRead(reader))
                    throw new Exception("The Log Event is not a valid log4j Xml block.");

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
                            case "log4j:message":
                                logMsg.Message = reader.ReadString();
                                break;

                            case "log4j:throwable":
                                logMsg.Message += Environment.NewLine + reader.ReadString();
                                break;

                            case "log4j:locationInfo":
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

                            case "log4j:properties":
                                reader.Read();
                                while (reader.MoveToContent() == XmlNodeType.Element
                                       && reader.Name == "log4j:data")
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
            var reader = XmlReader.Create(stream, _xmlSettings, this._xmlContext);
            var canParse = this.CanRead(reader);
            stream.Position = 0;
            return canParse;
        }
    }
}
