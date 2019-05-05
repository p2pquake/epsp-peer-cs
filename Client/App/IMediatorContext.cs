using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.App.State;
using Client.Client;
using Client.Common.Net;
using Client.Peer;

namespace Client.App
{
    /// <summary>
    /// EPSPピアとしての動作を制御するためのMediatorです。
    /// </summary>
    public interface IMediatorContext : IOperatable, IPeerState, IPeerConfig
    {
        // XXX: 外部から接続状態を書き換えられる．
        /// <summary>接続状態</summary>
        AbstractState State { get; set; }

        /// <summary>接続状態が変化すると発生します。接続状態は <see cref="State"/> で取得できます。</summary>
        event EventHandler StateChanged;
        /// <summary>操作が終了すると発生します。</summary>
        event EventHandler<OperationCompletedEventArgs> Completed;
        /// <summary>接続数が変化すると発生します。接続数は <see cref="IPeerState.Connections"/> で取得できます。</summary>
        event EventHandler ConnectionsChanged;

        /// <summary>地震情報を受信すると発生します。</summary>
        event EventHandler<EPSPQuakeEventArgs> OnEarthquake;
        /// <summary>津波予報を受信すると発生します。</summary>
        event EventHandler<EPSPTsunamiEventArgs> OnTsunami;
        /// <summary>地域ピア数を受信すると発生します。</summary>
        event EventHandler<EventArgs> OnAreapeers;
        /// <summary>緊急地震速報 配信試験(β)を受信すると発生します。</summary>
        event EventHandler<EPSPEEWTestEventArgs> OnEEWTest;
        /// <summary>地震感知情報を受信すると発生します。</summary>
        event EventHandler<EPSPUserquakeEventArgs> OnUserquake;

#if RAISE_RAW_DATA_EVENT
        event EventHandler<EPSPRawDataEventArgs> OnData;
#endif
#if MOBILE_SERVER
        void SendAll(Packet packet);
#endif

        /// <summary>地震感知情報を発信します。成功すると true が返ります。</summary>
        /// <returns>発信に成功したかどうか</returns>
        bool SendUserquake();
    }
}
