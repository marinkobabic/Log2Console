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
            this.ParseInternal(logStream.GenerateStreamFromString(), defaultLogger, actionLogMsg);
        }


        public virtual void Parse(Stream logStream, string defaultLogger, Action<LogMessage> actionLogMsg )
        {
            try
            {
                this.ParseInternal(logStream, defaultLogger, actionLogMsg);
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

        protected abstract void ParseInternal(Stream logStream, string defaultLogger, Action<LogMessage> actionLogMsg);

        public bool CanParse(Stream stream)
        {
            try
            {
                return this.CanParseInternal(stream);
            }
            catch (Exception )
            {
                return false;
            }
        }

        protected abstract bool CanParseInternal(Stream stream);
    }
}