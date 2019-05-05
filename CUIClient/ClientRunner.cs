using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Client.App;
using CUIClient.Handler;

namespace CUIClient
{
    /// <summary>
    /// EPSPピアの操作サンプルクラスです。
    /// 
    /// 各種情報の処理は <see cref="EPSPHandler"/> クラスが行います。
    /// </summary>
    class ClientRunner
    {
        private IMediatorContext mediatorContext;

        /// <summary>EPSPネットワークへの接続を開始します。<see cref="Stop"/>メソッドが実行されるまで接続を維持します。</summary>
        /// <param name="port">接続受け入れポート (未指定時はポート非開放)</param>
        public void Start(int port = -1)
        {
            mediatorContext = new MediatorContext();
            var epspHandler = new EPSPHandler(mediatorContext.CalcNowProtocolTime, () => mediatorContext.AreaPeerDictionary);

            mediatorContext.ConnectionsChanged += MediatorContext_ConnectionsChanged;
            mediatorContext.StateChanged += MediatorContext_StateChanged;
            mediatorContext.Completed += MediatorContext_Completed;

            mediatorContext.OnAreapeers += epspHandler.MediatorContext_OnAreapeers;
            mediatorContext.OnEarthquake += epspHandler.MediatorContext_OnEarthquake;
            mediatorContext.OnTsunami += epspHandler.MediatorContext_OnTsunami;
            mediatorContext.OnEEWTest += epspHandler.MediatorContext_OnEEWTest;
            mediatorContext.OnUserquake += epspHandler.MediatorContext_OnUserquake;

            if (port > 0)
            {
                mediatorContext.IsPortOpen = true;
                mediatorContext.Port = port;
            }

            mediatorContext.Connect();
        }

        /// <summary>
        /// EPSPネットワークとの接続を切断します。
        /// ただし、オペレーティングシステムのシャットダウン等を考慮し、切断処理が3秒を超えた場合は強制切断します。
        /// </summary>
        public void Stop()
        {
            Task.Factory.StartNew(() => mediatorContext.Disconnect());

            var stopWatch = Stopwatch.StartNew();
            while (true)
            {
                if (mediatorContext.CanConnect)
                {
                    break;
                }
                if (stopWatch.ElapsedMilliseconds >= 3000)
                {
                    break;
                }
                Thread.Sleep(100);
            }
            stopWatch.Stop();
        }

        /// <summary>
        /// 操作完了時のイベント処理
        ///
        /// <seealso cref="IMediatorContext.Completed"/>
        /// </summary>
        private void MediatorContext_Completed(object sender, Client.Client.OperationCompletedEventArgs e)
        {
            // 何もしない
        }

        /// <summary>
        /// 状態変化時のイベント処理
        /// 
        /// <seealso cref="IMediatorContext.StateChanged"/>
        /// </summary>
        private void MediatorContext_StateChanged(object sender, EventArgs e)
        {
            // 現在の状態を出力する

            var dictionary = new Dictionary<string, string>() {
                { "ConnectedState",     "接続済み" },
                { "ConnectingState",    "接続中" },
                { "DisconnectedStae",   "未接続" },
                { "DisconnectingState", "切断中" },
                { "MaintenanceState",   "接続維持処理中" }
            };

            var className = mediatorContext.State.GetType().Name;
            if (!dictionary.ContainsKey(className))
            {
                return;
            }
            
            if (className == "ConnectedState")
            {
                Console.WriteLine(
                    "{0} 状態: {1} (ピア数: {2}, 接続数: {3}, ポート開放: {4})",
                    GetDateTime(),
                    dictionary[className],
                    mediatorContext.PeerCount,
                    mediatorContext.Connections,
                    mediatorContext.IsPortOpened ? "はい" : "いいえ"
                    );
            }
            else
            {
                Console.WriteLine(
                    "{0} 状態: {1}",
                    GetDateTime(),
                    dictionary[className]
                    );
            }
        }

        /// <summary>
        /// 接続数変化時のイベント処理
        /// 
        /// <seealso cref="IMediatorContext.ConnectionsChanged"/>
        /// </summary>
        private void MediatorContext_ConnectionsChanged(object sender, EventArgs e)
        {
            Console.WriteLine("{0} 接続数: {1}", GetDateTime(), mediatorContext.Connections);
        }

        private string GetDateTime()
        {
            return DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff");
        }
    }
}
