using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.App;
using Client.Client.State;

namespace Client.Client
{
    interface IClientContextForState : IClientContext
    {
        AbstractState State { set; }
        IPeerStateForClient PeerState { get; }
    }
}
