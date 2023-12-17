using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.App.State;
using Client.App.Userquake;
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
        ReadonlyAbstractState ReadonlyState { get; }
        // XXX: 外部から接続状態を書き換えられる．
        /// <summary>接続状態</summary>
        AbstractState State { set; }

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
        /// <summary>地域ピア数を受信すると発生します（ EPSP ネットワークのパケット情報を含むイベント）。</summary>
        event EventHandler<EPSPAreapeersEventArgs> OnAreapeersReceived;
        /// <summary>緊急地震速報 配信試験(β)を受信すると発生します。</summary>
        event EventHandler<EPSPEEWTestEventArgs> OnEEWTest;
        /// <summary>地震感知情報を受信すると発生します。</summary>
        event EventHandler<EPSPUserquakeEventArgs> OnUserquake;
        /// <summary>地震感知情報を評価した結果、地震の可能性が高い場合に発生します。</summary>
        event EventHandler<UserquakeEvaluateEventArgs> OnNewUserquakeEvaluation;
        /// <summary>地震感知情報の評価結果が更新された場合に発生します。</summary>
        event EventHandler<UserquakeEvaluateEventArgs> OnUpdateUserquakeEvaluation;
        /// <summary>新規データ受信時に発生します。</summary>
        event EventHandler<EPSPRawDataEventArgs> OnData;
#if MOBILE_SERVER
        void SendAll(Packet packet);
#endif

        /// <summary>地震感知情報を発信します。成功すると true が返ります。</summary>
        /// <returns>発信に成功したかどうか</returns>
        bool SendUserquake();
    }
}
