using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Client.Common.General;
using Client.Common.Net;

using Client.Peer.Manager;
using Client.App;

#if MOBILE
using Client.Mobile;
#endif

namespace Client.Peer
{
    class Context : IPeerContext, IPeerConnector
    {
        private PeerManager peerManager;

        // TODO: FIXME: イベント発行はまだ未実装。
        public event EventHandler<EPSPQuakeEventArgs> OnEarthquake;
        public event EventHandler<EPSPTsunamiEventArgs> OnTsunami;
        public event EventHandler<EPSPAreapeersEventArgs> OnAreapeers;
        public event EventHandler<EPSPUserquakeEventArgs> OnUserquake;
        public event EventHandler<EventArgs> ConnectionsChanged;

        public IPeerState PeerState { private get; set; }
        public int Connections { get { return peerManager.Connections; } }

        public Context()
        {
            peerManager = new PeerManager();
            peerManager.PeerId += () => { return PeerState.PeerId; };

            peerManager.ConnectionsChanged += PeerManager_ConnectionsChanged;

            peerManager.OnAreapeers += PeerManager_OnAreapeers;
            peerManager.OnEarthquake += PeerManager_OnEarthquake;
            peerManager.OnTsunami += PeerManager_OnTsunami;
            peerManager.OnUserquake += PeerManager_OnUserquake;
        }

        private void PeerManager_OnUserquake(object sender, EPSPUserquakeEventArgs e)
        {
            OnUserquake(sender, e);
        }

        private void PeerManager_OnTsunami(object sender, EPSPTsunamiEventArgs e)
        {
            OnTsunami(sender, e);
        }

        private void PeerManager_OnEarthquake(object sender, EPSPQuakeEventArgs e)
        {
            OnEarthquake(sender, e);
        }

        private void PeerManager_OnAreapeers(object sender, EPSPAreapeersEventArgs e)
        {
            OnAreapeers(sender, e);
        }

        private void PeerManager_ConnectionsChanged(object sender, EventArgs e)
        {
            ConnectionsChanged(sender, e);
        }

        public bool Listen(int port)
        {
            // TODO: FIXME:
            throw new NotImplementedException();
        }

        public int[] Connect(PeerData[] peers)
        {
            List<int> connectedPeerIdList = new List<int>();

            foreach (PeerData peer in peers)
            {
                // TODO: 最大接続数ハードコーディング
                if (peerManager.Connections >= 4)
                {
                    break;
                }

                if (peerManager.Connect(peer))
                {
                    connectedPeerIdList.Add(peer.PeerId);
                }
            }

            return connectedPeerIdList.ToArray();
        }

        public void SendAll(Packet packet)
        {
            peerManager.Send(packet);
        }

        public void DisconnectAll()
        {
            peerManager.DisconnectAll();
        }

        public bool EndListen()
        {
            // TODO: FIXME:
            throw new NotImplementedException();
        }
    }
}
