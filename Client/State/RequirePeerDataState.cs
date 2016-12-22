using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Client.Client.General;
using Client.Common.General;
using Client.Common.Net;

namespace Client.Client.State
{
    /// <summary>
    /// ピアデータ要求のフェーズ
    /// </summary>
    class RequirePeerDataState : AbstractState
    {
        ClientConst.ProcessType processType;

        public RequirePeerDataState(ClientConst.ProcessType processType)
        {
            this.processType = processType;
        }

        public override void Process(Context context, CRLFSocket socket)
        {
            if (context.PeerId <= 0)
                throw new InvalidOperationException("ピアIDが設定されていません。");

            socket.WriteLine("115 1 " + context.PeerId);
        }

        public override void ReceiveJoiningPeerData(Context context, CRLFSocket socket, Packet packet)
        {
            IList<PeerData> peerList = new List<PeerData>();
            foreach (string peer in packet.Data)
            {
                string[] peerData = peer.Split(new char[] { ',' });
                peerList.Add(new PeerData(peerData[0], int.Parse(peerData[1]), int.Parse(peerData[2])));
            }

            int[] connectedPeerIds = context.ConnectToPeers(peerList.ToArray());

            socket.WriteLine("155 1 " + string.Join(":", Array.ConvertAll(connectedPeerIds, delegate(int v) { return v.ToString(); })));

            if (processType == ClientConst.ProcessType.Join)
            {
                context.State = new RequirePeerIdState();
            }
            else if (processType == ClientConst.ProcessType.Maintain)
            {
                context.State = new RequireProtocolTimeState();
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
