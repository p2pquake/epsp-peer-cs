using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.App.State;
using Client.Peer;

namespace Client.App
{
    public interface IMediatorContext : IOperatable, IPeerState, IPeerConfig
    {
        /// <summary>接続状態の変化</summary>
        event EventHandler StateChanged;
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

    }
}
