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

        public ParserBase GetParser(string logStream)
        {
            return this.GetParser(logStream.GenerateStreamFromString());
        }

        private static List<ParserBase> GetAllParsers()
        {
            var parsers = new List<ParserBase>();
            parsers.Add(new Log4j2Parser());
            parsers.Add(new Log4jParser());
            return parsers;
        }

        public ParserBase GetParser(Stream logStream)
        {
            return allParsers.FirstOrDefault(p => p.CanParse(logStream));
        }
    }
}
