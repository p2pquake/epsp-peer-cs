using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Client.App.State;
using Client.Client;
using Client.Client.General;
using Client.Common.General;
using Client.Peer;

namespace Client.App
{
    public class MediatorContext : IMediatorContext, IOperatable, IPeerState, IPeerStateForClient, IPeerStateForPeer
    {
        private IClientContext clientContext;
        private IPeerContext peerContext;

        public event EventHandler StateChanged;
        public event EventHandler ConnectionsChanged;
        public event EventHandler<EPSPQuakeEventArgs> OnEarthquake;
        public event EventHandler<EPSPTsunamiEventArgs> OnTsunami;
        public event EventHandler<EPSPAreapeersEventArgs> OnAreapeers;
        public event EventHandler<EPSPUserquakeEventArgs> OnUserquake;

        public IDictionary<string, int> AreaPeerDictionary { get; set; }
        public ClientState ClientState { get { return clientContext.ClientState; } }
        public int Connections { get { return peerContext.Connections; } }
        public bool IsPortOpened { get; set; }
        public KeyData Key { get; set; }
        public int PeerId { get; set; }
        public TimeSpan TimeOffset { get; set; }

        public bool CanConnect { get { return (clientContext.ClientState == ClientState.Disconnected); } }
        public bool CanDisconnect { get { return (clientContext.ClientState == ClientState.Connected); } }
        
        public MediatorContext()
        {
            // TODO: FIXME: インスタンス生成が必要（まだクラス作ってない）
            clientContext.PeerConnector = peerContext;
            clientContext.PeerState = this;
            clientContext.StateChanged += (s, e) => { StateChanged(s, e); };

            peerContext.PeerState = this;
            peerContext.ConnectionsChanged += (s,e) => { ConnectionsChanged(s, e); };
            peerContext.OnAreapeers += (s, e) => { OnAreapeers(s, e); };
            peerContext.OnUserquake += (s, e) => { OnUserquake(s, e); };
            peerContext.OnTsunami += (s, e) => { OnTsunami(s, e); };
            peerContext.OnEarthquake += (s, e) => { OnEarthquake(s, e); };
        }

        public bool Connect()
        {
            if (!CanConnect)
            {
                return false;
            }
            
            return clientContext.Join();
        }

        public bool Disconnect()
        {
            if (!CanDisconnect)
            {
                return false;
            }

            clientContext.Part();
            return true;
        }
    }
}
