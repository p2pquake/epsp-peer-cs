using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using Client.Common.General;
using System.IO;

namespace Client.Common.Net
{
    class ReadLineEventArgs : EventArgs
    {
        public string line;
        public Packet packet;
    }

    enum ConnectionState
    {
        Closed,
        Closing,
        Connecting,
        Connected,
        Error
    }

	class CRLFSocket
	{
        // CRLFSocket共通の接続タイムアウト
        private static int connectTimeout = 5000;

        public static int ConnectTimeout
        {
            get { return connectTimeout; }
            set { connectTimeout = value; }
        }

        // イベント
        public event EventHandler<ReadLineEventArgs> ReadLine = delegate(object s, ReadLineEventArgs e) { };
        public event EventHandler Closed = delegate(object s, EventArgs e) { };

        // CRLF分割用のバッファ
        private byte[] buffer;

        // ソケット
        private ISocket socket;
        private byte[] receiveBuffer;
        private const int BUFFER_SIZE = 1024;

        // 非同期のシグナル
        private ManualResetEvent connectEvent = new ManualResetEvent(false);
        
        public CRLFSocket() : this(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
        {
        }

        public CRLFSocket(Socket socket)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            this.socket = new SocketAdapter(socket);
            buffer = new byte[0];
        }

        public ConnectionState State
        {
            get
            {
                if (socket.Connected)
                {
                    return ConnectionState.Connected;
                }
                else
                {
                    return ConnectionState.Closed;
                }
            }
        }

        public IPEndPoint RemoteEndPoint
        {
            get
            {
                if (socket.Connected)
                    return (IPEndPoint)socket.RemoteEndPoint;
                else
                    return null;
            }
        }

        public bool Close()
        {
            if (!socket.Connected)
            {
                return false;
            }

            try
            {
                socket.Shutdown(SocketShutdown.Both);
            }
            catch (ObjectDisposedException)
            {
                return false;
            }
            finally
            {
                socket.Close();
            }
            return true;
        }

        public bool Connect(IPEndPoint ipEndPoint)
        {
            return Connect(ipEndPoint.Address.ToString(), ipEndPoint.Port);
        }

        public bool Connect(string host, int port)
        {
            if (State == ConnectionState.Closed)
            {
                try
                {
                    connectEvent.Reset();
                    IAsyncResult ar = socket.BeginConnect(host, port, new AsyncCallback(ConnectCallback), socket);

                    if (connectEvent.WaitOne(ConnectTimeout, true))
                    {
                        // Success
                        socket.EndConnect(ar);
                        Logger.GetLog().Debug("接続しました: " + host + ":" + port);

                        BeginReceive();
                        return true;
                    }
                    else
                    {
                        // Fail
                        try
                        {
                            socket.Close();
                            socket.EndConnect(ar);
                        }
                        catch (SocketException) { }
                        catch (ObjectDisposedException) { }

                        Logger.GetLog().Debug("タイムアウト: " + host + ":" + port);
                        return false;
                    }
                }
                catch (SocketException se)
                {
                    Logger.GetLog().Info("接続エラー: " + host + ":" + port, se);

                    return false;
                }
            }
            else
            {
                Logger.GetLog().Debug("接続要求を受け入れ可能な状態ではありません。");

                return false;
            }
        }

        public void BeginReceive()
        {
            // Hook
            receiveBuffer = new byte[BUFFER_SIZE];
            socket.BeginReceive(receiveBuffer, 0, BUFFER_SIZE, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            Socket socket = (Socket)ar.AsyncState;

            if (socket != null && socket.Connected)
                connectEvent.Set();
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            Socket socket = (Socket)ar.AsyncState;
            int receiveBytes = 0;
            try
            {
                receiveBytes = socket.EndReceive(ar);
            }
            catch (SocketException)
            {
                receiveBytes = 0;
            }
            catch (ObjectDisposedException)
            {
                return;
            }

            if (receiveBytes > 0)
            {
                ProcessReceiveData(receiveBytes);

                // Connected を見ても BeginReceive するときに切れてることがあるので、諦めて try-catch で囲む
                // if (socket.Connected)
                try
                {
                    socket.BeginReceive(receiveBuffer, 0, BUFFER_SIZE, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);
                    return;
                }
                catch (SocketException)
                {
                    // noop (後続処理で切断してもらう)
                }
                catch (ObjectDisposedException)
                {
                    return;
                }
            }

            Logger.GetLog().Debug("切断されました。");

            try
            {
                if (socket.Connected)
                {
                    Logger.GetLog().Debug("Connected: true");
                    socket.Close();
                    Logger.GetLog().Debug("切断しました。");
                }

                Closed(this, EventArgs.Empty);
            }
            catch (Exception e)
            {
                Logger.GetLog().Error("切断処理中に例外が発生しました。", e);
            }

            Logger.GetLog().Debug("切断処理完了。");
        }

        private void ProcessReceiveData(int receiveBytes)
        {
            // concat
            Array.Resize(ref buffer, buffer.Length + receiveBytes);
            Array.Copy(receiveBuffer, 0, buffer, buffer.Length - receiveBytes, receiveBytes);

            // split
            // \r\nがあればカット．
            int start = buffer.Length - receiveBytes - 1;
            if (start < 0)
                start = 0;
            while (true)
            {
                bool cont = false;
                for (int i = start; i < buffer.Length - 1; i++)
                {
                    if (buffer[i] == 0x0D && buffer[i + 1] == 0x0A)
                    {
                        // extract.
                        byte[] line = new byte[i];
                        Array.Copy(buffer, line, i);

                        string lineStr = Encoding.GetEncoding(932).GetString(line);
                        ReadLineEventArgs readLineEventArgs = new ReadLineEventArgs();
                        readLineEventArgs.line = lineStr;
                        Logger.GetLog().Debug("受信データ: " + lineStr);

                        try
                        {
                            readLineEventArgs.packet = Packet.Parse(lineStr);

                            ReadLine(this, readLineEventArgs);
                        }
                        catch (FormatException)
                        {
                            if (lineStr != "")
                            {
                                Logger.GetLog().Warn("パケット解析に失敗しました。処理をスキップします。");
                            }
                        }

                        // truncate.
                        byte[] newBuffer = new byte[buffer.Length - i - 2];
                        Array.Copy(buffer, i + 2, newBuffer, 0, newBuffer.Length);
                        buffer = newBuffer;

                        cont = true;
                        break;
                    }
                }
                if (!cont)
                    break;
            }
        }

        public bool WriteLine(string line)
        {
            if (State == ConnectionState.Connected)
            {
                try
                {
                    Logger.GetLog().Debug("送信データ: " + line);

                    socket.Send(Encoding.GetEncoding(932).GetBytes(line + "\r\n"));
                    return true;
                }
                catch (Exception e) when (e is SocketException || e is ObjectDisposedException)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
	}
}
