using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Client.Common.Net
{
    class SocketAdapter : ISocket
    {
        private Socket socket;

        public SocketAdapter(Socket socket)
        {
            this.socket = socket;
        }

        public bool Connected { get { return socket.Connected; } }
        public EndPoint RemoteEndPoint { get { return socket.RemoteEndPoint; } }

        public IAsyncResult BeginConnect(string host, int port, AsyncCallback asyncCallback, ISocket iSocket)
        {
            return socket.BeginConnect(host, port, asyncCallback, socket);
        }

        public void BeginReceive(byte[] buffer, int offset, int size, SocketFlags socketFlags, AsyncCallback asyncCallback, ISocket iSocket)
        {
            socket.BeginReceive(buffer, offset, size, socketFlags, asyncCallback, socket);
        }

        public void Close()
        {
            socket.Close();
        }

        public void EndConnect(IAsyncResult ar)
        {
            socket.EndConnect(ar);
        }

        public void Send(byte[] buffer)
        {
            socket.Send(buffer);
        }

        public void Shutdown(SocketShutdown both)
        {
            socket.Shutdown(both);
        }
    }
}
