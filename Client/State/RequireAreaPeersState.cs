using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Client.Common.Net;

namespace Client.Client.State
{
    class RequireAreaPeersState : AbstractState
    {
        public override void Process(Context context, CRLFSocket socket)
        {
            socket.WriteLine("127 1");
        }

        public override void NoticeAreaPeers(Context context, CRLFSocket socket, Packet packet)
        {
            // TODO: 地域ピア数の情報を捨ててる

            context.State = new RequireProtocolTimeState();
        }
    }
}
