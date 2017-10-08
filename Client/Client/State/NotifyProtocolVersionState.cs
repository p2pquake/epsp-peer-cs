using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Client.Client.General;
using Client.Common.Net;
using Client.Common.General;

namespace Client.Client.State
{
    /// <summary>
    /// プロトコルバージョンの通知フェーズ
    /// </summary>
    class NotifyProtocolVersionState : AbstractState
    {
        ClientConst.ProcessType processType;

        public NotifyProtocolVersionState(ClientConst.ProcessType processType)
        {
            this.processType = processType;
        }

        public override void Process(IClientContextForState context, CRLFSocket socket)
        {
            string[] datas = { Const.PROTOCOL_VERSION, Const.SOFTWARE_NAME, Const.SOFTWARE_VERSION };
            socket.WriteLine("131 1 " + string.Join(":", datas));
        }


        public override void NoticeAllowVersion(IClientContextForState context, CRLFSocket socket, Packet packet)
        {
            string serverVersion = packet.Data[0];

            // TODO: サーバのプロトコルバージョンチェックしてない

            if (processType == ClientConst.ProcessType.Join)
            {
                context.State = new RequireTemporaryPeerIdState();
            }
            else if (processType == ClientConst.ProcessType.Maintain)
            {
                context.State = new PeerEchoState();
            }
            else if (processType == ClientConst.ProcessType.Part)
            {
                throw new NotImplementedException();
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }
}
