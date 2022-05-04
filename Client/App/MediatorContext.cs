using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Client.App.State;
using Client.App.Userquake;
using Client.Client;
using Client.Client.General;
using Client.Common.General;
using Client.Common.Net;
using Client.Common.Net.UPnP;
using Client.Peer;
using Client.Peer.General;
using PKCSPeerCrypto;

namespace Client.App
{
    /// <summary>
    /// IMediatorContextの実装です。
    /// 
    /// <para><see cref="IOperatable.Connect"/> の接続処理は非同期で行われます。
    /// 処理結果は <see cref="IMediatorContext.Completed"/> により通知しますが、接続に失敗しても一定間隔で再接続を試み続けます。</para>
    /// <para><see cref="IOperatable.Disconnect"/> の切断処理は非同期で行われます。
    /// 切断が完了すると、 <see cref="IMediatorContext.State"/> の値が <see cref="DisconnectedState"/> に変化します。</para>
    /// </summary>
    public class MediatorContext : IMediatorContext, IOperatable, IPeerState, IPeerStateForClient, IPeerStateForPeer, IPeerConfig
    {
        private IClientContext clientContext;
        private IPeerContext peerContext;
        private MaintainTimer maintainTimer;
        private Aggregator userquakeAggregator;

        public event EventHandler StateChanged = (s, e) => { };
        public event EventHandler<OperationCompletedEventArgs> Completed = (s, e) => { };
        public event EventHandler ConnectionsChanged = (s, e) => { };
        public event EventHandler<EPSPQuakeEventArgs> OnEarthquake = (s, e) => { };
        public event EventHandler<EPSPTsunamiEventArgs> OnTsunami = (s, e) => { };
        public event EventHandler<EventArgs> OnAreapeers = (s, e) => { };
        public event EventHandler<EPSPEEWTestEventArgs> OnEEWTest = (s, e) => { };
        public event EventHandler<EPSPUserquakeEventArgs> OnUserquake = (s, e) => { };
        public event EventHandler<UserquakeEvaluateEventArgs> OnNewUserquakeEvaluation = delegate { };
        public event EventHandler<UserquakeEvaluateEventArgs> OnUpdateUserquakeEvaluation = delegate { };
        public event EventHandler<EPSPRawDataEventArgs> OnData = (s, e) => { };

        private AbstractState state;

        public ReadonlyAbstractState ReadonlyState {
            get => state;
        }
        public AbstractState State {
            set { state = value; StateChanged(this, EventArgs.Empty);  }
        }

        private IDictionary<string, int> areaPeerDictionary;
        
        public IDictionary<string, int> AreaPeerDictionary
        {
            get { return areaPeerDictionary; }
            set { areaPeerDictionary = value; OnAreapeers(this, EventArgs.Empty); }
        }

        public int PeerCount {
            get { return AreaPeerDictionary == null ? 0 : AreaPeerDictionary.Sum(e => e.Value);  }
        }
        public int Connections { get { return peerContext.Connections; } }
        public bool IsPortOpened { get; set; }
        public KeyData Key { get; set; }
        public int PeerId { get; set; }
        public TimeSpan TimeOffset { get; set; }

        public bool Verification { get; set; }
        public int AreaCode { get; set; }
        public string FormattedAreaCode { get { return AreaCode.ToString("D3"); } }
        public bool IsPortOpen { get; set; }
        public int Port { get; set; }
        public bool UseUPnP { get; set; }
        public int MaxConnections { get; set; }
        public bool IsPortListening { get; set; }

        public bool CanConnect { get { return state is DisconnectedState; } }
        public bool CanDisconnect { get { return state is ConnectedState; } }
        private bool CanMaintain { get { return CanDisconnect; } }

        /// <summary>
        /// インスタンスを初期化します。初期値はそれぞれ下記となります。
        /// <list type="bullet">
        ///   <item>
        ///     <description><see cref="AreaCode"/> (地域コード): 900 (地域未設定)</description>
        ///   </item>
        ///   <item>
        ///     <description><see cref="MaxConnections"/> (最大接続数): 4</description>
        ///   </item>
        /// </list>
        /// </summary>
        public MediatorContext()
        {
            clientContext = new ClientContext();
            peerContext = new Context();
            maintainTimer = new MaintainTimer(this, clientContext);
            userquakeAggregator = new Aggregator();
            state = new DisconnectedState();

            Verification = true;
            AreaCode = 900;
            MaxConnections = 4;
            IsPortOpen = false;
            UseUPnP = false;
            
            clientContext.PeerConfig = this;
            clientContext.PeerConnector = peerContext;
            clientContext.PeerState = this;
            clientContext.StateChanged += ClientContext_StateChanged;
            clientContext.OperationCompleted += ClientContext_OperationCompleted;

            peerContext.PeerConfig = this;
            peerContext.PeerState = this;
            peerContext.ConnectionsChanged += (s,e) => { ConnectionsChanged(s, e); };
            peerContext.OnUserquake += (s, e) => { if (!Verification || e.IsValid) { OnUserquake(s, e); } };
            peerContext.OnTsunami += (s, e) => { if (!Verification || e.IsValid) { OnTsunami(s, e); } };
            peerContext.OnEarthquake += (s, e) => { if (!Verification || e.IsValid) { OnEarthquake(s, e); } };
            peerContext.OnEEWTest += (s, e) => { if (!Verification || e.IsValid) { OnEEWTest(s, e); } };
            peerContext.OnData += (s, e) => { OnData(s, e); };

            maintainTimer.RequireConnect += MaintainTimer_RequireConnect;
            maintainTimer.RequireMaintain += MaintainTimer_RequireMaintain;
            maintainTimer.RequireDisconnect += MaintainTimer_RequireDisconnect;
            maintainTimer.RequireDisconnectAllPeers += MaintainTimer_RequireDisconnectAllPeers;

            OnUserquake += (s, e) => {
                if (areaPeerDictionary != null)
                    userquakeAggregator.AddUserquake(e.ReceivedAt, e.AreaCode, new Dictionary<string, int>(areaPeerDictionary));
            };
            userquakeAggregator.OnNew += (s, e) => { OnNewUserquakeEvaluation(this, e); };
            userquakeAggregator.OnUpdate += (s, e) => { OnUpdateUserquakeEvaluation(this, e); };
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
            state.Disconnect(this, clientContext, peerContext);
        }

        private void MaintainTimer_RequireConnect(object sender, EventArgs e)
        {
            if (!CanConnect) { return; }

            if (IsPortOpen)
            {
                if (UseUPnP)
                {
                    UPnPUtil.OpenPort(Port);
                }
                IsPortListening = peerContext.Listen(Port);
            }
            else
            {
                IsPortListening = false;
            }
            state.Connect(this, clientContext, peerContext);
        }

        private void MaintainTimer_RequireMaintain(object sender, EventArgs e)
        {
            if (!CanMaintain) { return; }

            state.Maintain(this, clientContext, peerContext);
        }

        private void ClientContext_StateChanged(object sender, EventArgs e)
        {
            // Do nothing.
            Logger.GetLog().Debug("ClientContext_StateChanged: " + clientContext.State.ToString());
        }

        private void ClientContext_OperationCompleted(object sender, OperationCompletedEventArgs e)
        {
            Logger.GetLog().Debug("ClientContext_OperationCompleted: " + e.Result.ToString() + ", " + e.ErrorCode.ToString());

            state.Completed(this, clientContext, peerContext, e);
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
