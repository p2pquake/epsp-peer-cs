using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Client.Common.Net;
using log4net;

namespace BridgeClient
{
    class ReceiveEventArgs : EventArgs
    {
        public Packet Packet { get; set; }
    }

    class MobileServer
    {
        public event EventHandler<ReceiveEventArgs> OnReceive;

        private static ILog logger = LogManager.GetLogger("MobileServer");
        private Thread thread;

        public void Start()
        {
            logger.Debug("HTTPサーバを開始します.");
            thread = new Thread(Run);
            thread.Start();
        }

        public void Stop()
        {
            thread.Abort();
        }

        private void Run()
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:6922/"); // プレフィックスの登録
            listener.Start();

            while (true)
            {
                HttpListenerContext context = listener.GetContext();
                HttpListenerRequest req = context.Request;
                HttpListenerResponse res = context.Response;

                logger.Debug("リクエストパラメタ: " + req.RawUrl);

                string send = req.QueryString.Get("send");

                if (send == null)
                {
                    res.Close();
                    continue;
                }

                logger.Debug("送信要求がありました: " + send);

                try
                {
                    Packet packet = Packet.Parse(send);
                    ReceiveEventArgs e = new ReceiveEventArgs();
                    e.Packet = packet;
                    OnReceive(this, e);
                }
                catch (Exception e)
                {
                    logger.Warn("送信に失敗しています.", e);
                }


                res.Close();
            }
        }
    }

}
