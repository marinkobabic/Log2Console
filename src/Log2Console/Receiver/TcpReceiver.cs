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

        int _port = 4505;
        [Category("Configuration")]
        [DisplayName("TCP Port Number")]
        [DefaultValue(4505)]
        public int Port
        {
            get { return _port; }
            set { _port = value; }
        }

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
            if (_tcpClient != null) return;

            Task.Run(() => this.InitializeAsync());
        }

        private void InitializeAsync()
        {
            TcpListener listener = new TcpListener(_ipv6 ? IPAddress.IPv6Any : IPAddress.Any, _port);
            listener.ExclusiveAddressUse = true;
            listener.Start(100);

            _tcpClient = listener.AcceptTcpClient();
            _tcpClient.ReceiveBufferSize = _bufferSize;

            Task.Run(() => this.Start());
        }
        private void Start()
        {
            try
            {
                using (_tcpClient)
                {

                    while (_tcpClient != null)
                    {
                        if (!_tcpClient.Connected)
                        {
                            this.Terminate();
                            Thread.Sleep(TimeSpan.FromSeconds(1));
                            this.Initialize();
                            return;
                        }

                        var ns = this.GetStream(_tcpClient);
                   
                        var action = new Action<LogMessage>(logMsg =>
                        {
                            logMsg.LoggerName = string.Format(":{1}.{0}", logMsg.LoggerName, _port);

                            Notifiable?.Notify(logMsg);
                        });
                        this.GetParser(ns).Parse(ns, "TcpLogger", action);
                    }
                }

            }
            catch (IOException ioException)
            {
                Console.WriteLine(ioException);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private MemoryStream GetStream(TcpClient tcpClient)
        {
            using (NetworkStream stream = tcpClient.GetStream())
            {
                byte[] data = new byte[1024];
                var ms = new MemoryStream();

                int numBytesRead;
                while (stream.DataAvailable && (numBytesRead = stream.Read(data, 0, data.Length)) > 0)
                {
                    ms.Write(data, 0, numBytesRead);
                }

                var test = ms.GenerateStringFromStream(Encoding.Default);
                var jou = test.Split('\0');
                ms.Position = 0;
                return ms;
            }
        }

        public override void Terminate()
        {
            if (_tcpClient == null) return;

            _tcpClient.Close();
            _tcpClient = null;
        }

        #endregion
    }
}
