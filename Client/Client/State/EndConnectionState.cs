using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Client.Client.General;
using Client.Common.Net;

namespace Client.Client.State
{
    class EndConnectionState : AbstractState
    {
        private ClientConst.OperationResult operationResult;
        private ClientConst.ErrorCode errorCode;

        public EndConnectionState(ClientConst.OperationResult operationResult, ClientConst.ErrorCode errorCode)
        {
            this.operationResult = operationResult;
            this.errorCode = errorCode;
        }

        public override void Process(Context context, CRLFSocket socket)
        {
            socket.WriteLine("119 1");
        }

        public override void EndConnection(Context context, CRLFSocket socket, Packet packet)
        {
            socket.Close();

            context.State = new FinishedState(operationResult, errorCode);
        }
    }
}
