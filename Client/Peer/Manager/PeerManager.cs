using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Client.Common.General;
using Client.Common.Net;
using Client.Peer.General;

namespace Client.Peer.Manager
{
    class PeerManager
    {
        private List<Peer> peerList;
        private DuplicateRemover duplicateRemover;
        
        public event EventHandler<EventArgs> ConnectionsChanged;
        public event EventHandler<EPSPQuakeEventArgs> OnEarthquake;
        public event EventHandler<EPSPTsunamiEventArgs> OnTsunami;
        public event EventHandler<EPSPAreapeersEventArgs> OnAreapeers;
        public event EventHandler<EPSPUserquakeEventArgs> OnUserquake;

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
                ConnectionsChanged(this, EventArgs.Empty);
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
                raiseDataEvent(e.packet);

                e.packet.Hop++;
                Send(e.packet, (Peer)sender);
            }
        }

        private void raiseDataEvent(Packet packet)
        {
            // TODO: FIXME: 署名検証をしていない
            if (packet.Code == Code.EARTHQUAKE)
            {

            }
            if (packet.Code == Code.TSUNAMI)
            {

            }
            if (packet.Code == Code.AREAPEERS)
            {
                if (packet.Data == null || packet.Data.Length < 2)
                {
                    return;
                }

                string[] data = packet.Data[2].Split(';');
                EPSPAreapeersEventArgs e = new EPSPAreapeersEventArgs();
                e.AreaPeerDictionary = data.ToDictionary(e => e.Split(',')[0], e => int.Parse(e.Split(',')[1]));
                OnAreapeers(this, e);
            }
            if (packet.Code == Code.USERQUAKE)
            {
                if (packet.Data == null || packet.Data.Length < 6)
                {
                    return;
                }

                string[] data = packet.Data[5].Split(',');
                if (data.Length < 2)
                {
                    return;
                }

                EPSPUserquakeEventArgs e = new EPSPUserquakeEventArgs();
                e.PublicKey = packet.Data[2];
                e.AreaCode = data[1];
                OnUserquake(this, e);
            }
        }

        void peer_Closed(object sender, EventArgs e)
        {
            peerList.Remove((Peer)sender);
            ConnectionsChanged(this, EventArgs.Empty);
        }

        internal void DisconnectAll()
        {
            foreach (Peer peer in peerList)
            {
                peer.Disconnect();
            }
            peerList.Clear();
            ConnectionsChanged(this, EventArgs.Empty);
        }
    }
}
