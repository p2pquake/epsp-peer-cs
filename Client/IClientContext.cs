using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.App;
using Client.Peer;

namespace Client.Client
{
    interface IClientContext
    {
        IPeerState PeerState { get; set; }

        IPeerConnector PeerConnector { get; set; }

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
