using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Client.App;

namespace Client.Client.State
{
    class PeerEchoState : AbstractState
    {
        public override void Process(IClientContextForState context, Common.Net.CRLFSocket socket)
        {
            IPeerStateForClient peerState = context.PeerState;
            string[] datas = { peerState.PeerId.ToString(), peerState.Connections.ToString() };
            socket.WriteLine("123 1 " + string.Join(":", datas));
        }

        public override void AcceptedEcho(IClientContextForState context, Common.Net.CRLFSocket socket, Common.Net.Packet packet)
        {
            IPeerStateForClient peerState = context.PeerState;
            if (peerState.Key == null || peerState.Key.IsExpired(context.PeerState.CalcNowProtocolTime()))
            {
                context.State = new RequireReallocateKeyState();
            }
            else
            {
                // TODO: 接続数増加のしきい値が固定値になっている
                if (peerState.Connections <= 2)
                {
                    context.State = new RequirePeerDataState(General.ClientConst.ProcessType.Maintain);
                }
                else
                {
                    context.State = new RequireProtocolTimeState();
                }
            }
        }
    }
}
