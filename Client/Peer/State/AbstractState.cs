using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Client.Peer.Manager;

using Client.Common.General;
using Client.Common.Net;
using System.Globalization;

namespace Client.Peer.State
{
    abstract class AbstractState
    {
        public event EventHandler<ReadLineEventArgs> ReadLine = (s,e) => {};
        public event EventHandler<EventArgs> EchoReplied = (s, e)=> { };

        public virtual void NotifyEarthquake(Manager.Peer peer, CRLFSocket socket, Packet packet)
        {
            if (packet.Data == null || packet.Data.Length != 4)
            {
                return;
            }

            Relay(peer, socket, packet);
        }

        public virtual void NotifyTsunami(Manager.Peer peer, CRLFSocket socket, Packet packet)
        {
            if (packet.Data == null || packet.Data.Length != 3)
            {
                return;
            }

            Relay(peer, socket, packet);
        }

        public virtual void NotifyUserQuake(Manager.Peer peer, CRLFSocket socket, Packet packet)
        {
            if (packet.Data == null || packet.Data.Length != 6)
            {
                return;
            }

            Relay(peer, socket, packet);
        }

        public virtual void NotifyBbs(Manager.Peer peer, CRLFSocket socket, Packet packet)
        {
            if (packet.Data == null || packet.Data.Length != 6)
            {
                return;
            }

            Relay(peer, socket, packet);
        }

        public virtual void NotifyAreaPeer(Manager.Peer peer, CRLFSocket socket, Packet packet)
        {
            if (packet.Data == null || packet.Data.Length != 3)
            {
                return;
            }

            Relay(peer, socket, packet);
        }

        public virtual void RequireInquiryEcho(Manager.Peer peer, CRLFSocket socket, Packet packet)
        {
            if (packet.Data == null || packet.Data.Length != 2)
            {
                return;
            }


            Relay(peer, socket, packet);
        }

        public virtual void ReplyInquiryEcho(Manager.Peer peer, CRLFSocket socket, Packet packet)
        {
            if (packet.Data == null || packet.Data.Length != 5)
            {
                return;
            }

            Relay(peer, socket, packet);
        }
        
        // 614 Server->Client
        public virtual void RequireProtocolVersion(Manager.Peer peer, CRLFSocket socket, Packet packet)
        {
            if (packet.Data == null || packet.Data.Length != 3)
            {
                return;
            }

            double version = 0.0;
            if (!double.TryParse(packet.Data[0], NumberStyles.Any, NumberFormatInfo.InvariantInfo, out version)) {
                return;
            }

            if (version < Const.ALLOW_PROTOCOL_VERSION)
            {
                socket.WriteLine("694 1 Protocol_version_incompatible");
                socket.Close();
                return;
            }
            
            string[] datas = { Const.PROTOCOL_VERSION, Const.SOFTWARE_NAME, Const.SOFTWARE_VERSION };
            socket.WriteLine("634 1 " + string.Join(":", datas));
        }

        // 612 Server->Client
        public virtual void RequirePeerId(Manager.Peer peer, CRLFSocket socket, Packet packet)
        {
            socket.WriteLine("632 1 " + peer.GetParentPeerId());
        }

        // 634 Client->Server
        public virtual void ReplyProtocolVersion(Manager.Peer peer, CRLFSocket socket, Packet packet)
        {
            if (packet.Data == null || packet.Data.Length != 3)
            {
                return;
            }

            double version = 0.0;
            if (!double.TryParse(packet.Data[0], NumberStyles.Any, NumberFormatInfo.InvariantInfo, out version))
            {
                return;
            }

            if (version < Const.ALLOW_PROTOCOL_VERSION)
            {
                socket.WriteLine("694 1 Protocol_version_incompatible");
                socket.Close();
                return;
            }
            
            socket.WriteLine("612 1");
        }

        // 632 Client->Server
        public virtual void ReplyPeerId(Manager.Peer peer, CRLFSocket socket, Packet packet)
        {
            if (packet.Data == null || packet.Data.Length != 1)
            {
                return;
            }

            int peerId = 0;
            if (!int.TryParse(packet.Data[0], out peerId))
            {
                return;
            }

            // FIXME: 多重接続チェックしてない.
            peer.PeerData.PeerId = peerId;
        }

        public virtual void RequirePeerEcho(Manager.Peer peer, CRLFSocket socket, Packet packet)
        {
            socket.WriteLine("631 1");
        }

        public virtual void ReplyPeerEcho(Manager.Peer peer, CRLFSocket socket, Packet packet)
        {
            EchoReplied(this, EventArgs.Empty);
        }

        public virtual void InvalidProtocolVersion(Manager.Peer peer, CRLFSocket socket, Packet packet)
        {
            socket.Close();
        }

        public virtual void InvalidOperation(Manager.Peer peer, CRLFSocket socket, Packet packet)
        {
            socket.Close();
        }

        public virtual void ReceiveReservedCode(Manager.Peer peer, CRLFSocket socket, Packet packet)
        {
            if (packet.Data == null || packet.Data.Length <= 0)
            {
                return;
            }

            Relay(peer, socket, packet);
        }

        private void Relay(Manager.Peer peer, CRLFSocket socket, Packet packet)
        {
            ReadLineEventArgs eventArgs = new ReadLineEventArgs();
            eventArgs.packet = packet;

            ReadLine(this, eventArgs);
        }
    }
}
