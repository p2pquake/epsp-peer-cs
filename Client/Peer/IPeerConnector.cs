using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.Common.General;

namespace Client.Peer
{
    /// <summary>
    /// 上位クラス・隣接クラスへ見せるPeerConnectorインタフェース
    /// </summary>
    interface IPeerConnector
    {
        /// <summary>
        /// ピア接続数が変化したことを通知します。
        /// </summary>
        event EventHandler<EventArgs> ConnectionsChanged;

        /// <summary>
        /// 指定したピアへの接続を試行します。
        /// </summary>
        /// <param name="peers">ピア情報</param>
        /// <returns>接続したピアのピアID</returns>
        int[] Connect(PeerData[] peers);

        /// <summary>接続数</summary>
        int Connections { get; }
    }
}
