using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Client.Common.Net;

namespace Client.Client.State
{
    class RequireReallocateKeyState : AbstractState
    {
        public override void Process(Context context, Common.Net.CRLFSocket socket)
        {
            string[] datas = { context.PeerId.ToString(), "Unknown" };
            if (context.Key != null)
            {
                datas[1] = context.Key.PrivateKey;
            }

            socket.WriteLine("124 1 " + string.Join(":", datas));
        }

        public override void ReAllocateKey(Context context, CRLFSocket socket, Packet packet)
        {
            RequireAllocateKeyState allocateKeyState = new RequireAllocateKeyState();
            context.Key = allocateKeyState.CreateKeyData(packet);

            ChangeState(context);
        }

        public override void KeyCantAllocateError(Context context, CRLFSocket socket, Packet packet)
        {
            ChangeState(context);
        }

        private void ChangeState(Context context)
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
