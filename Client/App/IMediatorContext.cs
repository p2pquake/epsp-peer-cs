using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.App.State;
using Client.Client;
using Client.Peer;

namespace Client.App
{
    public interface IMediatorContext : IOperatable, IPeerState, IPeerConfig
    {
        /// <summary>状態</summary>
        AbstractState State { get; set; }

        /// <summary>接続状態の変化</summary>
        event EventHandler StateChanged;
        /// <summary>完了状態への遷移</summary>
        event EventHandler<OperationCompletedEventArgs> Completed;
        /// <summary>接続数の変化</summary>
        event EventHandler ConnectionsChanged;

        /// <summary>地震情報イベント</summary>
        event EventHandler<EPSPQuakeEventArgs> OnEarthquake;
        /// <summary>津波予報イベント</summary>
        event EventHandler<EPSPTsunamiEventArgs> OnTsunami;
        /// <summary>地域ピア数イベント</summary>
        event EventHandler<EPSPAreapeersEventArgs> OnAreapeers;
        /// <summary>地震感知情報イベント</summary>
        event EventHandler<EPSPUserquakeEventArgs> OnUserquake;

#if RAISE_RAW_DATA_EVENT
        event EventHandler<EPSPRawDataEventArgs> OnData;
#endif
    }
}
