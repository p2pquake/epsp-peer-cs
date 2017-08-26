using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Client.Client.State
{
    class PeerEchoState : AbstractState
    {
        public override void Process(Context context, Common.Net.CRLFSocket socket)
        {
            string[] datas = { context.PeerId.ToString(), context.GetCurrentConnection().ToString() };
            socket.WriteLine("123 1 " + string.Join(":", datas));
        }

        public override void AcceptedEcho(Context context, Common.Net.CRLFSocket socket, Common.Net.Packet packet)
        {
            if (context.Key == null || context.Key.IsExpired())
            {
                context.State = new RequireReallocateKeyState();
            }
            else
            {
                if (context.GetCurrentConnection() <= 2)
                {
                    context.State = new RequirePeerDataState(General.ClientConst.ProcessType.Maintain);
                }
                else
                {
                    context.State = new RequireProtocolTimeState();
                }
            }
        }
    }
}
