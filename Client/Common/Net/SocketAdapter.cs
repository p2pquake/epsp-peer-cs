using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Client.Common.Net
{
    public class SocketAdapter : ISocket
    {
        private Socket socket;

        public SocketAdapter(Socket socket)
        {
            this.socket = socket;
        }

        public virtual bool Connected { get { return socket.Connected; } }
        public virtual EndPoint RemoteEndPoint { get { return socket.RemoteEndPoint; } }

        public virtual IAsyncResult BeginConnect(string host, int port, AsyncCallback asyncCallback, ISocket iSocket)
        {
            return socket.BeginConnect(host, port, asyncCallback, socket);
        }

        public virtual void BeginReceive(byte[] buffer, int offset, int size, SocketFlags socketFlags, AsyncCallback asyncCallback, ISocket iSocket)
        {
            socket.BeginReceive(buffer, offset, size, socketFlags, asyncCallback, socket);
        }

        public virtual void Close()
        {
            socket.Close();
        }

        public virtual void EndConnect(IAsyncResult ar)
        {
            socket.EndConnect(ar);
        }

        public virtual void Send(byte[] buffer)
        {
            socket.Send(buffer);
        }

        public virtual void Shutdown(SocketShutdown both)
        {
            socket.Shutdown(both);
        }
    }
}
