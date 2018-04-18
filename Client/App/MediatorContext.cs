using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Client.App.State;
using Client.Client;
using Client.Client.General;
using Client.Common.General;
using Client.Common.Net;
using Client.Peer;
using Client.Peer.General;
using PKCSPeerCrypto;

namespace Client.App
{
    public class MediatorContext : IMediatorContext, IOperatable, IPeerState, IPeerStateForClient, IPeerStateForPeer, IPeerConfig
    {
        private IClientContext clientContext;
        private IPeerContext peerContext;
        private MaintainTimer maintainTimer;

        public event EventHandler StateChanged = (s, e) => { };
        public event EventHandler<OperationCompletedEventArgs> Completed = (s, e) => { };
        public event EventHandler ConnectionsChanged = (s, e) => { };
        public event EventHandler<EPSPQuakeEventArgs> OnEarthquake = (s, e) => { };
        public event EventHandler<EPSPTsunamiEventArgs> OnTsunami = (s, e) => { };
        public event EventHandler<EPSPAreapeersEventArgs> OnAreapeers = (s, e) => { };
        public event EventHandler<EPSPEEWTestEventArgs> OnEEWTest = (s, e) => { };
        public event EventHandler<EPSPUserquakeEventArgs> OnUserquake = (s, e) => { };
#if RAISE_RAW_DATA_EVENT
        public event EventHandler<EPSPRawDataEventArgs> OnData = (s, e) => { };
#endif

        private AbstractState state;

        public AbstractState State {
            get { return state; }
            set { state = value; StateChanged(this, EventArgs.Empty);  }
        }

        public IDictionary<string, int> AreaPeerDictionary { get; set; }
        public int PeerCount {
            get { return AreaPeerDictionary == null ? 0 : AreaPeerDictionary.Sum(e => e.Value);  }
        }
        public int Connections { get { return peerContext.Connections; } }
        public bool IsPortOpened { get; set; }
        public KeyData Key { get; set; }
        public int PeerId { get; set; }
        public TimeSpan TimeOffset { get; set; }

        public int AreaCode { get; set; }
        public string FormattedAreaCode { get { return AreaCode.ToString("D3"); } }
        public bool IsPortOpen { get; set; }
        public int Port { get; set; }
        public int MaxConnections { get; set; }

        public bool CanConnect { get { return State is DisconnectedState; } }
        public bool CanDisconnect { get { return State is ConnectedState; } }
        private bool CanMaintain { get { return CanDisconnect; } }
        
        public MediatorContext()
        {
            clientContext = new ClientContext();
            peerContext = new Context();
            maintainTimer = new MaintainTimer(this, clientContext);
            state = new DisconnectedState();
            
            clientContext.PeerConfig = this;
            clientContext.PeerConnector = peerContext;
            clientContext.PeerState = this;
            clientContext.StateChanged += ClientContext_StateChanged;
            clientContext.OperationCompleted += ClientContext_OperationCompleted;

            peerContext.PeerConfig = this;
            peerContext.PeerState = this;
            peerContext.ConnectionsChanged += (s,e) => { ConnectionsChanged(s, e); };
            peerContext.OnAreapeers += (s, e) => { OnAreapeers(s, e); };
            peerContext.OnUserquake += (s, e) => { OnUserquake(s, e); };
            peerContext.OnTsunami += (s, e) => { OnTsunami(s, e); };
            peerContext.OnEarthquake += (s, e) => { OnEarthquake(s, e); };
            peerContext.OnEEWTest += (s, e) => { OnEEWTest(s, e); };
#if RAISE_RAW_DATA_EVENT
            peerContext.OnData += (s, e) => { OnData(s, e); };
#endif

            maintainTimer.RequireConnect += MaintainTimer_RequireConnect;
            maintainTimer.RequireMaintain += MaintainTimer_RequireMaintain;
            maintainTimer.RequireDisconnect += MaintainTimer_RequireDisconnect;
            maintainTimer.RequireDisconnectAllPeers += MaintainTimer_RequireDisconnectAllPeers;
        }

        private void MaintainTimer_RequireDisconnectAllPeers(object sender, EventArgs e)
        {
            peerContext.EndListen();
            peerContext.DisconnectAll();
        }

        private void MaintainTimer_RequireDisconnect(object sender, EventArgs e)
        {
            if (!CanDisconnect) { return; }

            peerContext.EndListen();
            State.Disconnect(this, clientContext, peerContext);
        }

        private void MaintainTimer_RequireConnect(object sender, EventArgs e)
        {
            if (!CanConnect) { return; }

            if (IsPortOpen)
            {
                peerContext.Listen(Port);
            }
            State.Connect(this, clientContext, peerContext);
        }

        private void MaintainTimer_RequireMaintain(object sender, EventArgs e)
        {
            if (!CanMaintain) { return; }

            State.Maintain(this, clientContext, peerContext);
        }

        private void ClientContext_StateChanged(object sender, EventArgs e)
        {
            // Do nothing.
            Logger.GetLog().Debug("ClientContext_StateChanged: " + clientContext.State.ToString());
        }

        private void ClientContext_OperationCompleted(object sender, OperationCompletedEventArgs e)
        {
            Logger.GetLog().Debug("ClientContext_OperationCompleted: " + e.Result.ToString() + ", " + e.ErrorCode.ToString());

            State.Completed(this, clientContext, peerContext, e);
            Completed(this, e);
        }

        public bool Connect()
        {
            if (!CanConnect)
            {
                return false;
            }

            peerContext.DisconnectAll();
            maintainTimer.Start();
            return true;
        }

        public bool Disconnect()
        {
            if (!CanDisconnect)
            {
                return false;
            }

            peerContext.DisconnectAll();
            maintainTimer.Stop();
            return true;
        }

        public DateTime CalcNowProtocolTime()
        {
            if (TimeOffset == null)
            {
                return DateTime.Now;
            }
            return DateTime.Now + TimeOffset;
        }

#if MOBILE_SERVER
        public void SendAll(Packet packet)
        {
            peerContext.SendAll(packet);
        }
#endif

        public bool SendUserquake()
        {
            if (Key == null)
            {
                return false;
            }
            if (Key.PrivateKey == null)
            {
                return false;
            }
            if (Key.IsExpired(CalcNowProtocolTime()))
            {
                return false;
            }
            if (PeerId <= 0)
            {
                return false;
            }
            if (AreaCode < 0)
            {
                return false;
            }

            var result = Signer.SignUserquake(PeerId, Key.PrivateKey, FormattedAreaCode, CalcNowProtocolTime());
            var packet = new Packet();
            packet.Code = Code.USERQUAKE;
            packet.Hop = 1;
            packet.Data = new string[] {
                result.signature,
                result.expire.ToString("yyyy/MM/dd HH-mm-ss"),
                Key.PublicKey,
                Key.Signature,
                Key.Expire.ToString("yyyy/MM/dd HH-mm-ss"),
                result.data
            };

            peerContext.SendAll(packet);
            return true;
        }
    }
}
