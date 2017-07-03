using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Log2Console.Log;
using Log2Console.Receiver.Parser;

namespace Log2Console.Receiver
{
    [Serializable]
    [DisplayName("TCP (IP v4 and v6)")]
    public class TcpReceiver : BaseReceiver
    {
        #region Port Property

        [NonSerialized]
        private TcpListener tcpListener;

        int _port = 4505;
        [Category("Configuration")]
        [DisplayName("TCP Port Number")]
        [DefaultValue(4505)]
        public int Port
        {
            get { return _port; }
            set { _port = value; }
        }

        [Category("Configuration")]
        [DisplayName("Logging Framework")]
        [DefaultValue(Receiver.ParserType.NLog)]

        public ParserType ParserType { get; set; }
        #endregion

        #region IpV6 Property

        bool _ipv6;
        [Category("Configuration")]
        [DisplayName("Use IPv6 Addresses")]
        [DefaultValue(false)]
        public bool IpV6
        {
            get { return _ipv6; }
            set { _ipv6 = value; }
        }

        private int _bufferSize = 10000;
        [Category("Configuration")]
        [DisplayName("Receive Buffer Size")]
        [DefaultValue(10000)]
        public int BufferSize
        {
            get { return _bufferSize; }
            set { _bufferSize = value; }
        }

        #endregion

        #region IReceiver Members

        [Browsable(false)]
        public override string SampleClientConfig
        {
            get
            {
                return
                    "Configuration for NLog:" + Environment.NewLine +
                    "<target name=\"TcpOutlet\" xsi:type=\"NLogViewer\" address=\"tcp://localhost:4505\"/>";
            }
        }

        [NonSerialized]
        TcpClient _tcpClient;

        public override void Initialize()
        {
            if (tcpListener != null) return;

            Task.Run(() => this.InitializeAsync());
        }

        private void InitializeAsync()
        {
            this.tcpListener = new TcpListener(_ipv6 ? IPAddress.IPv6Any : IPAddress.Any, _port);
            this.tcpListener.ExclusiveAddressUse = true;
            this.tcpListener.Start(100);

            _tcpClient = this.tcpListener.AcceptTcpClient();
            _tcpClient.ReceiveBufferSize = _bufferSize;

            Task.Run(() => this.Start());
        }

        private bool ReconnectIfNeeded()
        {
            if (_tcpClient != null && _tcpClient.Connected) return false;
            this.Terminate();
            Thread.Sleep(TimeSpan.FromSeconds(1));
            this.Initialize();
            return true;
        }

        private void Start()
        {
            try
            {
                using (_tcpClient)
                {

                    while (_tcpClient != null)
                    {
                        if (this.ReconnectIfNeeded())
                        {
                            return;
                        }

                        var action = new Action<LogMessage>(logMsg =>
                        {
                            logMsg.LoggerName = string.Format(":{1}.{0}", logMsg.LoggerName, _port);

                            Notifiable?.Notify(logMsg);
                        });

                        var networkStream = _tcpClient.GetStream();

                        if (networkStream.DataAvailable)
                        {
                            var parserInfo = this.GetParser(this.ParserType);
                            if (parserInfo == null)
                            {
                                this.ResetConnectionForce();
                                return;       
                            }
                            parserInfo.Stream = _tcpClient.GetStream();
                            parserInfo.Parser.Parse(parserInfo, "TcpLogger", action);
                        }
                        else
                        {
                            Thread.Sleep(TimeSpan.FromMilliseconds(300));
                            if (!networkStream.DataAvailable)
                            {
                                ResetConnectionForce();
                            }
                        }
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                this.ReconnectIfNeeded();
            }
        }

        private void ResetConnectionForce()
        {
            this._tcpClient.Close();
            this.ReconnectIfNeeded();
        }

        public override void Terminate()
        {
            if (_tcpClient == null) return;

            _tcpClient.Close();
            _tcpClient = null;

            this.tcpListener.Stop();
            this.tcpListener = null;
        }

        #endregion
    }
}
