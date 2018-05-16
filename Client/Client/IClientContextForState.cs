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
        new AbstractState State { set; }
        new IPeerConfig PeerConfig { get; }
        new IPeerStateForClient PeerState { get; }
        new IPeerConnector PeerConnector { get; }
    }
}
