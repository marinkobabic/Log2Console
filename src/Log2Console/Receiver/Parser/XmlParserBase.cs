using System;
using System.IO;
using System.Text;
using System.Xml;
using Log2Console.Log;
using Log2Console.Receiver.Parser;

namespace Log2Console.Receiver
{
    public abstract class XmlParserBase : ParserBase
    {
        protected static readonly DateTime s1970 = new DateTime(1970, 1, 1);

        /// <summary>
        /// We can share settings to improve performance
        /// </summary>
        private readonly XmlReaderSettings _xmlSettings = CreateSettings();

        /// <summary>
        /// We can share parser context to improve performance
        /// </summary>
        private XmlParserContext _xmlContext;

        private static XmlReaderSettings CreateSettings()
        {
            return new XmlReaderSettings
            {
                CloseInput = false,
                ValidationType = ValidationType.None,
                IgnoreComments = true,
                IgnoreWhitespace = true
            };
        }

        private XmlParserContext CreateContext()
        {
            if (_xmlContext != null)
            {
                return _xmlContext;
            }
            var nt = new NameTable();
            var nsmanager = new XmlNamespaceManager(nt);
            this.AddXmlNamespaces(nsmanager);
            _xmlContext = new XmlParserContext(nt, nsmanager, "elem", XmlSpace.None, Encoding.UTF8);
            return _xmlContext;
        }

        public override void Parse(ParserInfo parserInfo, string defaultLogger, Action<LogMessage> actionLogMsg)
        {
            if (parserInfo.Reader == null)
            {
                parserInfo.Reader = this.CreateReader(parserInfo.Stream);
                if (!this.CanRead(parserInfo.Reader))
                {
                    throw new Exception("Cannot parse the xml");
                }
            }
            base.Parse(parserInfo, defaultLogger, actionLogMsg);
        }

        protected virtual void AddXmlNamespaces(XmlNamespaceManager namespaceManager)
        {
            
        }

        protected bool CanRead(XmlReader reader)
        {
            try
            {
                if (string.IsNullOrEmpty(reader.Name) && reader.NodeType == XmlNodeType.None)
                {
                    reader.Read();
                    reader.MoveToContent();
                }

                return reader.NodeType == XmlNodeType.Element && reader.Name == this.GetEventName;
            }
            catch (Exception)
            {
                return false;
            }
        }

        protected abstract string GetEventName { get; }

        protected XmlReader CreateReader(Stream stream)
        {
            return XmlReader.Create(stream, _xmlSettings, this.CreateContext());
        }

        protected override ParserInfo CanParseInternal(Stream stream)
        {
            var parserInfo = new ParserInfo();
            parserInfo.Reader = this.CreateReader(stream);
            parserInfo.CanParse = this.CanRead((XmlReader)parserInfo.Reader);
            if (parserInfo.CanParse)
            {
                parserInfo.Parser = this;
            }
            else
            {
                parserInfo.Reader?.Dispose();
                parserInfo.Reader = null;
            }
            return parserInfo ;
        }
    }
}