using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Client.Common.Net;
using Client.Client.General;

namespace Client.Client.State
{
    class FinishedState : AbstractState, IFinishedState
    {
        public ClientConst.OperationResult Result { get; private set; }
        public ClientConst.ErrorCode ErrorCode { get; private set; }

        public FinishedState(ClientConst.OperationResult result, ClientConst.ErrorCode errorCode)
        {
            Result = result;
            ErrorCode = errorCode;
        }

        public override void Process(IClientContextForState context, CRLFSocket socket)
        {
            // 何もしない。
        }
    }
}
