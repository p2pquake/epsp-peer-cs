using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Client.General
{
    class ServerPoint
    {
        public string Host { get; set; }
        public int Port { get; set; }

        public ServerPoint(string host, int port)
        {
            Host = host;
            Port = port;
        }
    }
}
