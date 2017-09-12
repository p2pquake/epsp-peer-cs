using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.App;
using Client.Peer;

namespace Client.Client
{
    public enum ClientState
    {
        Disconnected,
        Connecting,
        Connected,
        Disconnecting
    }

    interface IClientContext
    {
        IPeerState PeerState { get; set; }

        IPeerConnector PeerConnector { get; set; }
        
        ClientState ClientState { get; }

        /// <summary>
        /// 参加する
        /// </summary>
        /// <returns></returns>
        bool Join();

        /// <summary>
        /// 参加終了する
        /// </summary>
        void Part();

        /// <summary>
        /// 接続を維持する
        /// </summary>
        /// <returns></returns>
        bool Maintain();
    }
}
