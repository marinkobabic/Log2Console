using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;

using Log2Console.Log;
using Log2Console.Receiver.Parser;

namespace Log2Console.Receiver
{
    public class Log4jParser : XmlParserBase
    {
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
        protected override void ParseInternal(ParserInfo parserInfo, string defaultLogger, Action<LogMessage> logMsgAction )
        {
            using (XmlReader reader = (XmlReader)parserInfo.Reader)
            {
                var logMsg = new LogMessage();

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

        protected override void AddXmlNamespaces(XmlNamespaceManager namespaceManager)
        {
            namespaceManager.AddNamespace("log4j", "http://jakarta.apache.org/log4j/");
            namespaceManager.AddNamespace("nlog", "http://nlog-project.org");
        }

        protected override string GetEventName => "log4j:event";
    }
}
