using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Client.Common.General;
using Client.Common.Net;

using Client.Peer.Manager;

#if MOBILE
using Client.Mobile;
#endif

namespace Client.Peer
{
    class Context
    {
        PeerManager peerManager;
#if MOBILE
        HTTPServer server;
#endif

        public Func<int> PeerId;

        internal Context()
        {
            peerManager = new PeerManager(this);
            peerManager.PeerId += () => { return PeerId(); };

#if MOBILE
            server = new HTTPServer(peerManager);
            server.start();
#endif
        }

        internal int[] ConnectTo(PeerData[] peerList)
        {
            List<int> connectedPeerIds = new List<int>();

            foreach (PeerData peer in peerList)
            {
                if (peerManager.Connect(peer))
                {
                    connectedPeerIds.Add(peer.PeerId);
                }

                // TODO: 最大接続数ハードコーディング
                if (peerManager.GetNumberOfConnection() >= 4)
                {
                    break;
                }
            }

            return connectedPeerIds.ToArray();
        }

        internal int GetNumberOfConnection()
        {
            return peerManager.GetNumberOfConnection();
        }

        internal void DisconnectAll()
        {
            peerManager.DisconnectAll();
        }

        internal void SendAll(Packet packet)
        {
            peerManager.Send(packet);
        }
    }
}
