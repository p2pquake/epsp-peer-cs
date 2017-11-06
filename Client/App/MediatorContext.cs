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
    public class MediatorContext : IMediatorContext, IOperatable, IPeerState, IPeerStateForClient, IPeerStateForPeer, IPeerConfig
    {
        private IClientContext clientContext;
        private IPeerContext peerContext;
        private MaintainTimer maintainTimer;
        
        public event EventHandler StateChanged;
        public event EventHandler<OperationCompletedEventArgs> Completed;
        public event EventHandler ConnectionsChanged;
        public event EventHandler<EPSPQuakeEventArgs> OnEarthquake;
        public event EventHandler<EPSPTsunamiEventArgs> OnTsunami;
        public event EventHandler<EPSPAreapeersEventArgs> OnAreapeers;
        public event EventHandler<EPSPUserquakeEventArgs> OnUserquake;

        private AbstractState state;

        public AbstractState State {
            get { return state; }
            set { state = value; StateChanged(this, EventArgs.Empty);  }
        }

        public IDictionary<string, int> AreaPeerDictionary { get; set; }
        public int Connections { get { return peerContext.Connections; } }
        public bool IsPortOpened { get; set; }
        public KeyData Key { get; set; }
        public int PeerId { get; set; }
        public TimeSpan TimeOffset { get; set; }

        public int AreaCode { get; set; }
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

            peerContext.PeerState = this;
            peerContext.ConnectionsChanged += (s,e) => { ConnectionsChanged(s, e); };
            peerContext.OnAreapeers += (s, e) => { OnAreapeers(s, e); };
            peerContext.OnUserquake += (s, e) => { OnUserquake(s, e); };
            peerContext.OnTsunami += (s, e) => { OnTsunami(s, e); };
            peerContext.OnEarthquake += (s, e) => { OnEarthquake(s, e); };

            maintainTimer.RequireConnect += MaintainTimer_RequireConnect;
            maintainTimer.RequireMaintain += MaintainTimer_RequireMaintain;
            maintainTimer.RequireDisconnect += MaintainTimer_RequireDisconnect;
        }

        private void MaintainTimer_RequireDisconnect(object sender, EventArgs e)
        {
            if (!CanDisconnect) { return; }

            State.Disconnect(this, clientContext, peerContext);
        }

        private void MaintainTimer_RequireConnect(object sender, EventArgs e)
        {
            if (!CanConnect) { return; }

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
    }
}
