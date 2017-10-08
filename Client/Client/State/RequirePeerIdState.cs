using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Client.App;
using Client.Common.Net;

namespace Client.Client.State
{
    class RequirePeerIdState : AbstractState
    {
        public override void Process(IClientContextForState context, CRLFSocket socket)
        {
            IPeerStateForClient peerState = context.PeerState;
            string[] datas = { peerState.PeerId.ToString(), context.Port.ToString(), context.AreaCode.ToString(), peerState.Connections.ToString(), context.AllowConnection.ToString() };

            socket.WriteLine("116 1 " + string.Join(":", datas));
        }

        public override void AllocatePeerId(IClientContextForState context, CRLFSocket socket, Packet packet)
        {
            // TODO: 鍵の取得は任意だが、必ず実施してしまっている
            context.State = new RequireAllocateKeyState();
        }
    }
}
