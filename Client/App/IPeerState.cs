using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.Client;
using Client.Client.General;

namespace Client.App
{
    /// <summary>
    /// ピアの状態を表すインタフェースです。
    /// </summary>
    public interface IPeerState
    {
        /// <summary>ピアID</summary>
        int PeerId { get; }
        /// <summary>プロトコル時刻とシステム時刻の差</summary>
        TimeSpan TimeOffset { get; }
        /// <summary>現在の接続数</summary>
        int Connections { get; }
        /// <summary>ポートが開放されているかどうか</summary>
        bool IsPortOpened { get; }
        /// <summary>情報発信用の鍵情報</summary>
        KeyData Key { get; }
        /// <summary>地域ピア数</summary>
        IDictionary<string, int> AreaPeerDictionary { get; }
        /// <summary>総ピア数</summary>
        int PeerCount { get; }
        /// <summary>プロトコル時刻を演算し、返します。</summary>
        DateTime CalcNowProtocolTime();
    }

    interface IPeerStateForClient
    {
        int PeerId { get; set; }

        TimeSpan TimeOffset { get; set; }

        int Connections { get; }

        bool IsPortOpened { set; }

        KeyData Key { get; set; }

        IDictionary<string, int> AreaPeerDictionary { get; set; }

        int PeerCount { get; }

        DateTime CalcNowProtocolTime();
    }

    interface IPeerStateForPeer
    {
        int PeerId { get; }

        TimeSpan TimeOffset { get; }

        KeyData Key { get; }

        IDictionary<string, int> AreaPeerDictionary { get; set; }

        int PeerCount { get; }

        DateTime CalcNowProtocolTime();
    }
}
