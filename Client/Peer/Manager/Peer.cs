using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using Client.Common.General;
using Client.Common.Net;

using Client.Peer.General;
using Client.Peer.State;


namespace Client.Peer.Manager
{
    class Peer
    {
        PeerManager peerManager;
        CRLFSocket socket;
        AbstractState state;

        internal PeerData PeerData { get; set; }
        public bool IsConnected { get { return socket.State == ConnectionState.Connected; } }

        public Func<int> PeerId;
        public int GetParentPeerId() { return PeerId(); }

        public event EventHandler Closed = (s,e)=>{};
        public event EventHandler<ReadLineEventArgs> ReadLine = (s, e) => { };

        public Peer(PeerManager peerManager) : this(peerManager, new CRLFSocket())
        {
        }
        
        internal Peer(PeerManager peerManager, CRLFSocket socket)
        {
            this.peerManager = peerManager;
            this.state = new GeneralState();
            this.socket = socket;

            state.ReadLine += new EventHandler<ReadLineEventArgs>(State_ReadLine);

            socket.Closed += new EventHandler(Socket_Closed);
            socket.ReadLine += new EventHandler<ReadLineEventArgs>(Socket_ReadLine);
        }

        internal void BeginReceive()
        {
            socket.BeginReceive();
        }

        void State_ReadLine(object sender, ReadLineEventArgs e)
        {
            ReadLine(this, e);
        }

        void Socket_ReadLine(object sender, ReadLineEventArgs e)
        {
            Packet packet = e.packet;
            string methodName = PeerConst.GetCodeName(packet.Code);

            if (methodName == null)
            {
                return;
            }

            Type type = state.GetType();
            MethodInfo methodInfo = type.GetMethod(methodName);

            object[] args = { this, socket, packet };

            methodInfo.Invoke(state, args);
        }

        void Socket_Closed(object sender, EventArgs e)
        {
            Logger.GetLog().Debug("ピアから切断されました: " + PeerData.PeerId);
            Closed(this, EventArgs.Empty);
        }

        internal bool Connect(PeerData peerData)
        {
            Logger.GetLog().Debug("次のピアに接続します: " + peerData.PeerId + "(" + peerData.Address + ":" + peerData.Port + ")");

            this.PeerData = peerData;
            bool result = socket.Connect(peerData.Address, peerData.Port);

            Logger.GetLog().Debug("接続結果: " + result);

            return result;
        }

        internal void Send(Packet packet)
        {
            socket.WriteLine(packet.ToPacketString());
        }

        internal void Disconnect()
        {
            socket.Close();
        }
    }
}
