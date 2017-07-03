using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Log2Console.Log;
using Log2Console.Receiver.Parser;

namespace Log2Console.Receiver
{
    public abstract class ParserBase
    {
        public virtual void Parse(string logStream, string defaultLogger, Action<LogMessage> actionLogMsg)
        {
            var parserInfo = new ParserInfo();
            parserInfo.Stream = logStream.GenerateStreamFromString();
            this.ParseInternal(parserInfo, defaultLogger, actionLogMsg);
        }

        internal virtual void ResetParserInfo(ParserInfo parserInfo)
        {
            
        }

        public virtual void Parse(ParserInfo parserInfo, string defaultLogger, Action<LogMessage> actionLogMsg)
        {
            try
            {
                this.ParseInternal(parserInfo, defaultLogger, actionLogMsg);
                parserInfo.Reader = null;
            }
            catch (Exception e)
            {
                var logMsg = new LogMessage
                {
                    // Create a simple log message with some default values
                    LoggerName = defaultLogger,
                    ThreadName = "NA",
                    Message = e.Message,
                    TimeStamp = DateTime.Now,
                    Level = LogLevels.Instance[LogLevel.Error],
                    ExceptionString = e.Message + Environment.NewLine + e.StackTrace
                };
                actionLogMsg(logMsg);
            }
        }

        protected abstract void ParseInternal(ParserInfo parserInfo, string defaultLogger, Action<LogMessage> actionLogMsg);

        public ParserInfo CanParse(Stream stream)
        {
            try
            {
                return this.CanParseInternal(stream);
            }
            catch (Exception)
            {
                return null;
            }
        }

        protected abstract ParserInfo CanParseInternal(Stream stream);
    }
}