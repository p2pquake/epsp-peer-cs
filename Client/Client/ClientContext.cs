using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.App;
using Client.Peer;

namespace Client.Client
{
    class ClientContext : IClientContext
    {
        public ClientState ClientState
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IPeerConnector PeerConnector
        {
            set
            {
                throw new NotImplementedException();
            }
        }

        public IPeerStateForClient PeerState
        {
            set
            {
                throw new NotImplementedException();
            }
        }

        public event EventHandler StateChanged;

        public bool Join()
        {
            throw new NotImplementedException();
        }

        public bool Maintain()
        {
            throw new NotImplementedException();
        }

        public void Part()
        {
            throw new NotImplementedException();
        }
    }
}
