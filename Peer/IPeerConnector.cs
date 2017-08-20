using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.Common.General;

namespace Client.Peer
{
    interface IPeerConnector
    {
        /// <summary>
        /// 指定したピアへの接続を試行します。
        /// </summary>
        /// <param name="peers">ピア情報</param>
        /// <returns>接続したピアのピアID</returns>
        int[] Connect(PeerData[] peers);

        /// <summary>
        /// 現在のピア接続数
        /// </summary>
        /// <returns></returns>
        int Connections { get; }
    }
}
