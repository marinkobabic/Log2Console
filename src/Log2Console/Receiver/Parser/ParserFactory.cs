using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Log2Console.Receiver.Parser
{
    public class ParserFactory
    {
        private static List<ParserBase> allParsers = GetAllParsers();

        public ParserInfo GetParserInfo(string logStream)
        {
            return this.GetParserInfo(logStream.GenerateStreamFromString());
        }

        private static List<ParserBase> GetAllParsers()
        {
            var parsers = new List<ParserBase>();
            parsers.Add(new Log4j2Parser());
            parsers.Add(new Log4jParser());
            parsers.Add(new Log4NetParser());
            return parsers;
        }

        public ParserInfo GetParserInfo(Stream logStream)
        {
            foreach (var parserBase in allParsers)
            {
                var parserInfo = parserBase.CanParse(logStream);
                if (parserInfo != null && parserInfo.CanParse)
                {
                    return parserInfo;
                }
            }
            return null;
        }

        public ParserInfo GetParserInfo(ParserType parserType)
        {
            var parserInfo = new ParserInfo();
            parserInfo.CanParse = true;
            switch (parserType)
            {
                case ParserType.Log4j2:
                    parserInfo.Parser = new Log4j2Parser();
                    break;
                case ParserType.Log4Net:
                    parserInfo.Parser = new Log4NetParser();
                    break;
                case ParserType.NLog:
                    parserInfo.Parser = new Log4jParser();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(parserType), parserType, null);
            }
            return parserInfo;
        }
    }
}
