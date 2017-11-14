using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Client.Peer.Manager;

using Client.Common.General;
using Client.Common.Net;

namespace Client.Peer.State
{
    abstract class AbstractState
    {
        public event EventHandler<ReadLineEventArgs> ReadLine = (s,e) => {};

        public virtual void NotifyEarthquake(Manager.Peer peer, CRLFSocket socket, Packet packet)
        {
            Relay(peer, socket, packet);
        }

        public virtual void NotifyTsunami(Manager.Peer peer, CRLFSocket socket, Packet packet)
        {
            Relay(peer, socket, packet);
        }

        public virtual void NotifyUserQuake(Manager.Peer peer, CRLFSocket socket, Packet packet)
        {
            Relay(peer, socket, packet);
        }

        public virtual void NotifyBbs(Manager.Peer peer, CRLFSocket socket, Packet packet)
        {
            Relay(peer, socket, packet);
        }

        public virtual void NotifyAreaPeer(Manager.Peer peer, CRLFSocket socket, Packet packet)
        {
            Relay(peer, socket, packet);
        }

        public virtual void RequireInquiryEcho(Manager.Peer peer, CRLFSocket socket, Packet packet)
        {
            // FIXME: あとで実装する.
            Relay(peer, socket, packet);
        }

        public virtual void ReplyInquiryEcho(Manager.Peer peer, CRLFSocket socket, Packet packet)
        {
            Relay(peer, socket, packet);
        }

        // 614 Server->Client
        public virtual void RequireProtocolVersion(Manager.Peer peer, CRLFSocket socket, Packet packet)
        {
            string[] datas = { Const.PROTOCOL_VERSION, Const.SOFTWARE_NAME, Const.SOFTWARE_VERSION };
            socket.WriteLine("634 1 " + string.Join(":", datas));
        }

        // 612 Server->Client
        public virtual void RequirePeerId(Manager.Peer peer, CRLFSocket socket, Packet packet)
        {
            socket.WriteLine("632 1 " + peer.GetPeerId());
        }

        // 634 Client->Server
        public virtual void ReplyProtocolVersion(Manager.Peer peer, CRLFSocket socket, Packet packet)
        {
            if (double.Parse(packet.Data[0]) < Const.ALLOW_PROTOCOL_VERSION)
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
            // FIXME: 多重接続チェックしてない.
            peer.PeerData.PeerId = int.Parse(packet.Data[0]);
        }

        public virtual void RequirePeerEcho(Manager.Peer peer, CRLFSocket socket, Packet packet)
        {
            socket.WriteLine("631 1");
        }

        public virtual void ReplyPeerEcho(Manager.Peer peer, CRLFSocket socket, Packet packet)
        {
            // 何もしない。
        }

        private void Relay(Manager.Peer peer, CRLFSocket socket, Packet packet)
        {
            ReadLineEventArgs eventArgs = new ReadLineEventArgs();
            eventArgs.packet = packet;

            ReadLine(this, eventArgs);
        }
    }
}
