using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Client.Peer.Manager;
using Client.Common.General;
using Client.Common.Net;

using System.Threading;
using System.Net;
using System.IO;

namespace Client.Mobile
{
    class HTTPServer
    {
        PeerManager peerManager;

        public HTTPServer(PeerManager peerManager)
        {
            this.peerManager = peerManager;
        }

        private Thread thread;

        public void start()
        {
            Logger.GetLog().Debug("HTTPサーバを開始します.");
            thread = new Thread(run);
            thread.Start();
        }

        public void stop()
        {
            thread.Abort();
        }

        private void run()
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:6922/"); // プレフィックスの登録
            listener.Start();

            while (true)
            {
                HttpListenerContext context = listener.GetContext();
                HttpListenerRequest req = context.Request;
                HttpListenerResponse res = context.Response;

                Logger.GetLog().Debug("リクエストパラメタ: " + req.RawUrl);

                string send = req.QueryString.Get("send");

                if (send == null)
                {
                    res.Close();
                    continue;
                }

                Logger.GetLog().Debug("送信要求がありました: " + send);

                try
                {
                    Packet packet = Packet.Parse(send);

                    peerManager.Send(packet);
                }
                catch (Exception e)
                {
                    Logger.GetLog().Warn("送信に失敗しています.", e);
                }


                res.Close();
            }
        }
    }
}
