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

    /// <summary>
    /// 上位クラスへ見せるClientContextインタフェース
    /// </summary>
    interface IClientContext
    {
        IPeerStateForClient PeerState { set; }

        IPeerConnector PeerConnector { set; }
        
        ClientState ClientState { get; }

        event EventHandler StateChanged;

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
