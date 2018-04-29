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
    class ClientRunner
    {
        private IMediatorContext mediatorContext;

        public void Start(int port = -1)
        {
            mediatorContext = new MediatorContext();
            var epspHandler = new EPSPHandler(mediatorContext.CalcNowProtocolTime);

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

        private void MediatorContext_Completed(object sender, Client.Client.OperationCompletedEventArgs e)
        {

        }

        private void MediatorContext_StateChanged(object sender, EventArgs e)
        {
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
