using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Client.Common.Net;

namespace Client.Client.State
{
    class RequireReallocateKeyState : AbstractState
    {
        public override void Process(IClientContextForState context, Common.Net.CRLFSocket socket)
        {
            string[] datas = { context.PeerId.ToString(), "Unknown" };
            if (context.Key != null)
            {
                // FIXME: PrivateKeyに値入っているっけ？
                datas[1] = context.PeerState.Key.PrivateKey;
            }

            socket.WriteLine("124 1 " + string.Join(":", datas));
        }

        public override void ReAllocateKey(IClientContextForState context, CRLFSocket socket, Packet packet)
        {
            RequireAllocateKeyState allocateKeyState = new RequireAllocateKeyState();
            context.Key = allocateKeyState.CreateKeyData(packet);

            ChangeState(context);
        }

        public override void KeyCantAllocateError(IClientContextForState context, CRLFSocket socket, Packet packet)
        {
            ChangeState(context);
        }

        private void ChangeState(IClientContextForState context)
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
