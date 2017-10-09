using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Client.Client.General;
using Client.Common.Net;

namespace Client.Client.State
{
    class RequireProtocolTimeState : AbstractState
    {
        public override void Process(IClientContextForState context, CRLFSocket socket)
        {
            socket.WriteLine("118 1");
        }

        public override void ReceiveProtocolTime(IClientContextForState context, CRLFSocket socket, Packet packet)
        {
            // FIXME: プロトコル時刻の情報を捨ててる

            context.State = new EndConnectionState(ClientConst.OperationResult.Successful, ClientConst.ErrorCode.SUCCESSFUL);
        }
    }
}
