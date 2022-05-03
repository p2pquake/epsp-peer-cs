using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Client.Common.General;
using Client.Common.Net;

using Client.Peer.Manager;
using Client.App;
using System.Net.Sockets;

namespace Client.Peer
{
    class Context : IPeerContext, IPeerConnector
    {
        private PeerManager peerManager;
        private AsyncListener asyncListener;
        
        public event EventHandler<EPSPQuakeEventArgs> OnEarthquake;
        public event EventHandler<EPSPTsunamiEventArgs> OnTsunami;
        public event EventHandler<EPSPEEWTestEventArgs> OnEEWTest;
        public event EventHandler<EPSPUserquakeEventArgs> OnUserquake;
        public event EventHandler<EPSPRawDataEventArgs> OnData;
        public event EventHandler<EventArgs> ConnectionsChanged;

        public IPeerConfig PeerConfig { private get; set; }
        public IPeerStateForPeer PeerState { private get; set; }
        public int Connections { get { return peerManager.Connections; } }

        public Context()
        {
            peerManager = new PeerManager();
            peerManager.PeerId += () => { return PeerState.PeerId; };
            peerManager.ProtocolTime += () => { return PeerState.CalcNowProtocolTime(); };
            peerManager.PeerCount += () => { return PeerState.PeerCount; };

            peerManager.ConnectionsChanged += PeerManager_ConnectionsChanged;

            peerManager.OnAreapeers += (s, e) => { if (!PeerConfig.Verification || e.IsValid) { PeerState.AreaPeerDictionary = e.AreaPeerDictionary; } };
            peerManager.OnEarthquake += PeerManager_OnEarthquake;
            peerManager.OnTsunami += PeerManager_OnTsunami;
            peerManager.OnEEWTest += PeerManager_OnEEWTest;
            peerManager.OnUserquake += PeerManager_OnUserquake;
            peerManager.OnData += PeerManager_OnData;
        }

        private void PeerManager_OnData(object sender, EPSPRawDataEventArgs e)
        {
            OnData(sender, e);
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

        private void PeerManager_OnEEWTest(object sender, EPSPEEWTestEventArgs e)
        {
            OnEEWTest(sender, e);
        }

        private void PeerManager_ConnectionsChanged(object sender, EventArgs e)
        {
            ConnectionsChanged(sender, e);
        }

        public bool Listen(int port)
        {
            EndListen();

            try
            {
                asyncListener = new AsyncListener(port);
                asyncListener.Accept += AsyncListener_Accept;
                asyncListener.Start();
            }
            catch (SocketException)
            {
                asyncListener = null;
                return false;
            }

            return true;
        }

        private void AsyncListener_Accept(object sender, AcceptEventArgs e)
        {
            if (Connections >= PeerConfig.MaxConnections && !e.Socket.RemoteEndPoint.ToString().Contains("127.0.0.1"))
            {
                e.Socket.Close();
                return;
            }

            peerManager.AddFromSocket(e.Socket);
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
            if (asyncListener == null)
            {
                return false;
            }

            asyncListener.Stop();
            return true;
        }
    }
}
