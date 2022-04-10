using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Client.Common.Net;

namespace Client.Client.State
{
    class RequireAreaPeersState : AbstractState
    {
        internal override void Process(IClientContextForState context, CRLFSocket socket)
        {
            socket.WriteLine("127 1");
        }

        internal override void NoticeAreaPeers(IClientContextForState context, CRLFSocket socket, Packet packet)
        {
            context.PeerState.AreaPeerDictionary = packet.Data[0].Split(';').ToDictionary(v => v.Split(',')[0], v => int.Parse(v.Split(',')[1]));
            context.State = new RequireProtocolTimeState();
        }
    }
}
