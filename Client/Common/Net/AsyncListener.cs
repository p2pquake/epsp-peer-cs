using Client.Common.General;

using System;
using System.Net;
using System.Net.Sockets;

namespace Client.Common.Net
{
    public class AcceptEventArgs : EventArgs
    {
        public Socket Socket { get; set; }
    }

    public class AsyncListener
    {
        bool interrupt;
        int port;

        TcpListener tcpListener;
        public event EventHandler<AcceptEventArgs> Accept = delegate (object s, AcceptEventArgs e) { };

        public AsyncListener(int port)
        {
            this.port = port;
            this.interrupt = false;

            Logger.GetLog().Debug("インスタンス初期化 (ポート: " + port + ")");
        }

        public void Start()
        {
            tcpListener = new TcpListener(IPAddress.Any, port);
            tcpListener.Start();
            AcceptInfinite();

            Logger.GetLog().Debug("リッスン (IPv4 TCP ポート: " + port + ")");
        }

        async private void AcceptInfinite()
        {
            while (!interrupt)
            {
                try
                {
                    Socket socket = await tcpListener.AcceptSocketAsync();
                    Logger.GetLog().Debug("接続受け入れ: " + socket.RemoteEndPoint.ToString());

                    var acceptEventArgs = new AcceptEventArgs();
                    acceptEventArgs.Socket = socket;
                    Accept(this, acceptEventArgs);
                }
                catch (ObjectDisposedException)
                {
                    Logger.GetLog().Debug("リッスン中断");
                }
                catch (SocketException ex)
                {
                    Logger.GetLog().Warn("対ピア接続受け入れ時にエラーが発生しました。無視します。", ex);
                }
            }

            Logger.GetLog().Debug("リッスン処理終了");
        }

        public void Stop()
        {
            Logger.GetLog().Debug("リッスン中断要求");

            interrupt = true;
            tcpListener.Stop();
        }
    }
}
