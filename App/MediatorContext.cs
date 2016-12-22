using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Client.App.State;
using Client.Client.General;
using Client.Common.General;

namespace Client.App
{
    class MediatorContext : IOperatable
    {
        private AbstractState state;

        public AbstractState State
        {
            get { return state; }
            set {
                state = value;
                StateChanged(this, EventArgs.Empty);
                Logger.GetLog().Debug("アプリケーションの状態が変化しました: " + state.GetType().Name);
            }
        }

        private Client.Context ClientContext { get; set; }
        public Peer.Context PeerContext { get; set; }
        public int PeerId
        {
            get { return ClientContext.PeerId; }
        }

        public event EventHandler StateChanged = (s, e) => { };

        public MediatorContext()
        {
            ClientContext = new Client.Context();
            ClientContext.StateChanged += new EventHandler(ClientContext_StateChanged);
            ClientContext.OperationCompleted += new EventHandler(ClientContext_OperationCompleted);
            ClientContext.ConnectToPeers += ClientContext_ConnectToPeers;
            ClientContext.GetCurrentConnection += ClientContext_GetCurrentConnection;
            ClientContext.NotifyCurrentPeers += ClientContext_NotifyCurrentPeers;

            PeerContext = new Peer.Context();
            PeerContext.PeerId += () => { return ClientContext.PeerId; };

            State = new DisconnectedState();

            new MaintainTimer(this).start();
        }

        #region ClientContextのイベントハンドラ

        void ClientContext_StateChanged(object sender, EventArgs e)
        {
            Logger.GetLog().Debug("クライアント状態が変化しました: " + ClientContext.State.GetType().Name);
        }
       
        void ClientContext_OperationCompleted(object sender, EventArgs e)
        {
            Logger.GetLog().Debug("クライアント処理が完了しました: " + ClientContext.getOperationResult().ToString());
            State.Completed(this, ClientContext, PeerContext);
        }

        int[] ClientContext_ConnectToPeers(PeerData[] peerList)
        {
            Logger.GetLog().Debug("ピア接続が要求されました。");

            int[] connectedPeers = PeerContext.ConnectTo(peerList);
            return connectedPeers;
        }

        internal int ClientContext_GetCurrentConnection()
        {
            return PeerContext.GetNumberOfConnection();
        }

        void ClientContext_NotifyCurrentPeers(int peers)
        {
            // 何もしない
        }

        #endregion

        #region IOperatable メンバー

        public bool CanConnect { get { return state.CanConnect; } }
        public bool CanDisconnect { get { return state.CanDisconnect; } }

        public bool Connect()
        {
            return state.Connect(this, ClientContext, PeerContext);
        }

        public bool Disconnect()
        {
            throw new NotImplementedException();
        }

        #endregion

        internal bool CanMaintain { get { return state.CanMaintain; } }

        internal bool Maintain()
        {
            return state.Maintain(this, ClientContext, PeerContext);
        }
    }
}
