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

        public IDictionary<string, int> AreaPeerDictionary { get; set; }
        public ClientState ClientState { get { return clientContext.ClientState; } }
        public int Connections { get { return peerContext.Connections; } }
        public bool IsPortOpened { get; set; }
        public KeyData Key { get; set; }
        public int PeerId { get; set; }
        public TimeSpan TimeOffset { get; set; }
        
        public MediatorContext()
        {
            // TODO: FIXME: インスタンス生成が必要（まだクラス作ってない）
            clientContext.PeerConnector = peerContext;
            clientContext.PeerState = this;
            peerContext.PeerState = this;
        }
        
    }
}
