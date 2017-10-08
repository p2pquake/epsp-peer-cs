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
        public override void Process(IClientContextForState context, CRLFSocket socket)
        {
            socket.WriteLine("117 1 " + context.PeerState.PeerId);
        }

        public override void AllocateKey(IClientContextForState context, CRLFSocket socket, Packet packet)
        {
            context.PeerState.Key = CreateKeyData(packet);
            context.State = new RequireAreaPeersState();
        }

        public override void KeyCantAllocateError(IClientContextForState context, CRLFSocket socket, Packet packet)
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
