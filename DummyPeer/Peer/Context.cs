using Client.App;
using Client.Common.General;
using Client.Common.Net;
using Client.Peer;
using Client.Peer.Manager;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DummyPeer.Peer
{
    /// <summary>
    /// Client.Peer.Context をベースに、単独動作を可能とした Context
    /// </summary>
    class Context
    {
        private PeerManager peerManager;
        private AsyncListener asyncListener;

        public Context()
        {
            peerManager = new PeerManager();
            peerManager.PeerId += () => 9999;
            peerManager.ProtocolTime += () => DateTime.Now;
            peerManager.PeerCount += () => 2;
            peerManager.ConnectionsChanged += (s, e) => { };
            peerManager.OnAreapeers += (s, e) => { };
            peerManager.OnEarthquake += (s, e) => { };
            peerManager.OnTsunami += (s, e) => { };
            peerManager.OnEEWTest += (s, e) => { };
            peerManager.OnUserquake += (s, e) => { };
        }

        public bool Listen(int port)
        {
            EndListen();

            asyncListener = new AsyncListener(port);
            asyncListener.Accept += AsyncListener_Accept;
            asyncListener.Start();

            return true;
        }

        private void AsyncListener_Accept(object sender, AcceptEventArgs e)
        {
            peerManager.AddFromSocket(e.Socket);
        }

        public bool Connect(PeerData peer)
        {
            return peerManager.Connect(peer);
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
            if (asyncListener == null)
            {
                return false;
            }

            asyncListener.Stop();
            return true;
        }
    }
}
