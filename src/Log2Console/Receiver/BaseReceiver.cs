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

        [NonSerialized]
        private ParserBase _parser;

        protected ParserBase GetParser(string logStream)
        {
            if (_parser != null)
            {
                return _parser;
            }
            var factory = new ParserFactory();
            _parser = factory.GetParser(logStream);
            return _parser;
        }

        protected ParserBase GetParser(Stream logStream)
        {
            if (_parser != null)
            {
                return _parser;
            }
            var factory = new ParserFactory();
            _parser = factory.GetParser(logStream);
            return _parser;
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
