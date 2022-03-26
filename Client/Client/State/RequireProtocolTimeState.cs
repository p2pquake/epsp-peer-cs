using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

using Client.Client.General;
using Client.Common.Net;

namespace Client.Client.State
{
    class RequireProtocolTimeState : AbstractState
    {
        internal override void Process(IClientContextForState context, CRLFSocket socket)
        {
            socket.WriteLine("118 1");
        }

        internal override void ReceiveProtocolTime(IClientContextForState context, CRLFSocket socket, Packet packet)
        {
            DateTime protocolTime = DateTime.ParseExact(packet.Data[0], "yyyy/MM/dd HH-mm-ss", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None);
            context.PeerState.TimeOffset = protocolTime - DateTime.Now;

            context.State = new EndConnectionState(ClientConst.OperationResult.Successful, ClientConst.ErrorCode.SUCCESSFUL);
        }
    }
}
