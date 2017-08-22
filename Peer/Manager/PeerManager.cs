using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Client.Common.General;
using Client.Common.Net;

namespace Client.Peer.Manager
{
    class PeerManager
    {
        private List<Peer> peerList;
        private DuplicateRemover duplicateRemover;

        public Func<int> PeerId { get; set; }

        internal int Connections { get { return peerList.Count; } }

        public PeerManager()
        {
            peerList = new List<Peer>();
            duplicateRemover = new DuplicateRemover();
        }

        public bool Connect(PeerData peerData)
        {
            Peer peer = new Peer(this);
            peer.Closed += new EventHandler(peer_Closed);
            peer.ReadLine += new EventHandler<ReadLineEventArgs>(peer_ReadLine);
            peer.PeerId += () => { return PeerId(); };

            bool result = peer.Connect(peerData);
            if (result)
            {
                peerList.Add(peer);
            }

            return result;
        }
        
        internal void Send(Packet packet)
        {
            Send(packet, null);
        }

        internal void Send(Packet packet, Peer exceptPeer)
        {
            foreach (Peer peer in peerList)
            {
                if (peer != exceptPeer)
                {
                    peer.Send(packet);
                }
            }
        }

        void peer_ReadLine(object sender, ReadLineEventArgs e)
        {
            if (!duplicateRemover.isDuplicate(e.packet))
            {
                e.packet.Hop++;
                Send(e.packet, (Peer)sender);
            }
        }

        void peer_Closed(object sender, EventArgs e)
        {
            peerList.Remove((Peer)sender);
        }

        internal void DisconnectAll()
        {
            foreach (Peer peer in peerList)
            {
                peer.Disconnect();
            }
            peerList.Clear();
        }
    }
}
