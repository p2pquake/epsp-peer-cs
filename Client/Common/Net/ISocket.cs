using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Client.Common.Net
{
    public interface ISocket
    {
        bool Connected { get; }
        EndPoint RemoteEndPoint { get; }

        void Shutdown(SocketShutdown both);
        void Close();
        IAsyncResult BeginConnect(string host, int port, AsyncCallback asyncCallback, ISocket iSocket);
        void EndConnect(IAsyncResult ar);
        void BeginReceive(byte[] buffer, int offset, int size, SocketFlags socketFlags, AsyncCallback asyncCallback, ISocket iSocket);
        void Send(byte[] buffer);
    }
}
