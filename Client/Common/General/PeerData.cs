using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Client.Common.General
{
    public class PeerData
    {
        private string address;
        private int port;
        private int peerId;

        public string Address { get { return address; } }
        public int Port { get { return port; } }
        public int PeerId { get { return peerId; } set { peerId = value; } }

        public PeerData(string address, int port, int peerId)
        {
            this.address = address;
            this.port = port;
            this.peerId = peerId;
        }
    }
}
