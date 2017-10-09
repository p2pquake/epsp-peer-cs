using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.App;
using Client.Client.State;
using Client.Peer;

namespace Client.Client
{
    interface IClientContextForState : IClientContext
    {
        AbstractState State { set; }
        IPeerConfig PeerConfig { get; }
        IPeerStateForClient PeerState { get; }
        IPeerConnector PeerConnector { get; }
    }
}
