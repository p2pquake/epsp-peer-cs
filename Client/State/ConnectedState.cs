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
        ClientConst.ProcessType processType;

        public ConnectedState(ClientConst.ProcessType processType)
        {
            this.processType = processType;
        }

        public override void Process(Context context, CRLFSocket socket)
        {
            Logger.GetLog().Debug("...再接続中かな？");
            // throw new NotSupportedException();
        }

        public override void RequireAllowVersion(Context context, CRLFSocket socket, Packet packet)
        {
            context.State = new NotifyProtocolVersionState(processType);
        }
    }
}
