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
        public override void Process(Context context, CRLFSocket socket)
        {
            socket.WriteLine("113 1");
        }

        public override void AllocateTemporaryPeerId(Context context, CRLFSocket socket, Packet packet)
        {
            string peerId = packet.Data[0];
            context.PeerId = int.Parse(peerId);

            // TODO: ポート開放の分岐がなく、常にポート閉鎖状態になっている
            context.State = new RequirePeerDataState(General.ClientConst.ProcessType.Join);
        }
    }
}
