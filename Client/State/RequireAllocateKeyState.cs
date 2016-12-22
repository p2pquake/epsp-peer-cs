using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

using Client.Client.General;
using Client.Common.Net;

namespace Client.Client.State
{
    class RequireAllocateKeyState : AbstractState
    {
        public override void Process(Context context, CRLFSocket socket)
        {
            socket.WriteLine("117 1 " + context.PeerId);
        }

        public override void AllocateKey(Context context, CRLFSocket socket, Packet packet)
        {
            context.Key = CreateKeyData(packet);
            context.State = new RequireAreaPeersState();
        }

        public override void KeyCantAllocateError(Context context, CRLFSocket socket, Packet packet)
        {
            context.State = new RequireAreaPeersState();
        }

        public KeyData CreateKeyData(Packet packet)
        {
            KeyData key = new KeyData();
            key.PrivateKey = packet.Data[0];
            key.PublicKey = packet.Data[1];
            key.Expire = DateTime.ParseExact(packet.Data[2], "yyyy/MM/dd HH-mm-ss", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None);
            key.Signature = packet.Data[3];

            return key;
        }
    }
}
