using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Client.Common.Net;

namespace Client.Client.State
{
    class RequirePeerIdState : AbstractState
    {
        public override void Process(Context context, CRLFSocket socket)
        {
            string[] datas = { context.PeerId.ToString(), context.Port.ToString(), context.AreaCode.ToString(), context.GetCurrentConnection().ToString(), context.AllowConnection.ToString() };

            socket.WriteLine("116 1 " + string.Join(":", datas));
        }

        public override void AllocatePeerId(Context context, CRLFSocket socket, Packet packet)
        {
            context.NotifyCurrentPeers(int.Parse(packet.Data[0]));

            // TODO: 鍵の取得は任意だが、必ず実施してしまっている
            context.State = new RequireAllocateKeyState();
        }
    }
}
