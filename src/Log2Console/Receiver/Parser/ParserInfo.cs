using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Log2Console.Receiver.Parser
{
    public class ParserInfo
    {
        public ParserBase Parser { get; internal set; }
        public XmlReader Reader { get; internal set; }

        public Stream Stream { get; internal set; }

        public bool CanParse { get; internal set; }
    }
}
