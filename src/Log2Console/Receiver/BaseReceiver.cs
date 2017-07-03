using System;
using System.ComponentModel;
using System.IO;
using Log2Console.Log;
using Log2Console.Receiver.Parser;


namespace Log2Console.Receiver
{
    [Serializable]
    public abstract class BaseReceiver : MarshalByRefObject, IReceiver
    {
        [NonSerialized]
        protected ILogMessageNotifiable Notifiable;

        [NonSerialized]
        private string _displayName;

        protected ParserInfo GetParser(string logStream)
        {
            var factory = new ParserFactory();
            return factory.GetParserInfo(logStream);
        }

        protected ParserInfo GetParser(ParserType parserType)
        {
            var factory = new ParserFactory();
            return factory.GetParserInfo(parserType);
        }

        protected ParserInfo GetParser(Stream logStream)
        {
            var factory = new ParserFactory();
            return factory.GetParserInfo(logStream);
        }

        #region IReceiver Members

        public abstract string SampleClientConfig { get; }

        [Browsable(false)]
        public string DisplayName
        {
            get { return _displayName; }
            protected set { _displayName = value; }
        }

        public abstract void Initialize();
        public abstract void Terminate();

        public virtual void Attach(ILogMessageNotifiable notifiable)
        {
            Notifiable = notifiable;
        }

        public virtual void Detach()
        {
            Notifiable = null;
        }

        #endregion
    }

}
