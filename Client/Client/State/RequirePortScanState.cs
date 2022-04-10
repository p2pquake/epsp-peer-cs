using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Client.Common.Net;

namespace Client.Client.State
{
    /// <summary>
    /// ポート開放チェック要求フェーズ
    /// </summary>
    class RequirePortScanState : AbstractState
    {
        internal override void Process(IClientContextForState context, CRLFSocket socket)
        {
            string[] datas = { context.PeerState.PeerId.ToString(), context.PeerConfig.Port.ToString() };
            socket.WriteLine("114 1 " + string.Join(":", datas));
        }

        internal override void ReceivePortCheckResult(IClientContextForState context, CRLFSocket socket, Packet packet)
        {
            context.PeerState.IsPortOpened = (packet.Data[0] == "1");

            context.State = new RequirePeerDataState(General.ClientConst.ProcessType.Join);
        }
    }
}
