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
    /// 接続直後のフェーズ
    /// </summary>
    class ConnectedState : AbstractState
    {
        public ClientConst.ProcessType ProcessType { get; }

        public ConnectedState(ClientConst.ProcessType processType)
        {
            ProcessType = processType;
        }

        public override void Process(IClientContextForState context, CRLFSocket socket)
        {
            Logger.GetLog().Debug("...再接続中かな？");
            // throw new NotSupportedException();
        }

        public override void RequireAllowVersion(IClientContextForState context, CRLFSocket socket, Packet packet)
        {
            context.State = new NotifyProtocolVersionState(ProcessType);
        }
    }
}
