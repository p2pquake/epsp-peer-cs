using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Client.Common.Net;

namespace Client.Client.State
{
    /// <summary>
    /// ピアIDの暫定割り当てフェーズ
    /// </summary>
    class RequireTemporaryPeerIdState : AbstractState
    {
        public override void Process(IClientContextForState context, CRLFSocket socket)
        {
            socket.WriteLine("113 1");
        }

        public override void AllocateTemporaryPeerId(IClientContextForState context, CRLFSocket socket, Packet packet)
        {
            string peerId = packet.Data[0];
            context.PeerState.PeerId = int.Parse(peerId);

            if (context.PeerConfig.IsPortOpen && context.PeerConfig.IsPortListening)
            {
                context.State = new RequirePortScanState();
                return;
            }

            context.PeerState.IsPortOpened = false;
            context.State = new RequirePeerDataState(General.ClientConst.ProcessType.Join);
        }
    }
}
