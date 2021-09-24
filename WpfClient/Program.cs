using Client.App;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace WpfClient
{
    public static class Program
    {
        // FIXME: 動作するか試している。あとでリファクタリングする。
        static MediatorContext client;
        static RootViewModel viewModel;

        [STAThread]
        public static void Main() {
            Task.Run(() => { BootP2PQuake(); });
            App app = new();
            app.InitializeComponent();
            app.Run();
        }

        public static void BootP2PQuake()
        {
            // ViewModel を取得したりイベントハンドラをくっつけたりする
            while (viewModel == null)
            {
                try
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        if (App.Current?.MainWindow?.DataContext != null)
                        {
                            var window = (MainWindow)App.Current?.MainWindow;
                            viewModel = (RootViewModel)window?.DataContext;
                            window.OnUserquake += Window_OnUserquake;
                        }
                    });
                }
                catch
                {
                    // nothing to do
                }

                Thread.Sleep(1000);
            };

            client = new MediatorContext();
            client.ConnectionsChanged += Client_ConnectionsChanged;
            client.StateChanged += Client_StateChanged;
            client.OnAreapeers += Client_OnAreapeers;

            client.Connect();
        }

        private static void Window_OnUserquake(object sender, EventArgs e)
        {
            client.SendUserquake();
        }

        private static void Client_ConnectionsChanged(object sender, EventArgs e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                viewModel.StatusViewModel.Connections = $"{client.Connections} / {client.MaxConnections} ピア";
            });
        }

        private static void Client_OnAreapeers(object sender, EventArgs e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                viewModel.NumberOfPeersLabel = $"{client.PeerCount} ピア";
                viewModel.StatusViewModel.NumberOfPeers = $"{client.PeerCount} ピア";

                viewModel.StatusViewModel.AreapeerText = String.Join("、 ", client.AreaPeerDictionary.Select((area) => $"{area.Key}: {area.Value}ピア"));
            });
        }

        private static void Client_StateChanged(object sender, EventArgs e)
        {
            // 未接続、切断中 = 切断アイコン
            var status = ChoiceByState(
                disconnected: "未接続",
                disconnecting: "切断中...",
                connecting: "接続中...",
                connected: "接続済み",
                noConnection: "接続なし"
                );

            var statusIconGlyph = ChoiceByState(
                disconnected: "\xF384",
                disconnecting: "\xF384",
                connecting: "\xF384",
                connected: "\xEC27",
                noConnection: "\xF384"
                );

            var statusTitle = ChoiceByState(
                disconnected: "サーバーに接続できません。",
                disconnecting: "切断しています...",
                connecting: "接続を開始しています...",
                connected: "正常に接続されています。",
                noConnection: "ピアと接続されていません。"
                );

            var statusDescription = ChoiceByState(
                disconnected: "サーバーに接続できないため、P2P地震情報のネットワークに接続できませんでした。自動的にリトライします。",
                disconnecting: "P2P地震情報のネットワークから切断しようとしています。",
                connecting: "P2P地震情報のネットワークへ接続しようとしています。",
                connected: "P2P地震情報のネットワークに接続されており、最新の情報をリアルタイムに受信できる状態です。",
                noConnection: "ピアと接続されていないため、最新の情報が受信できません。自動的にリトライします。"
                );

            var portStatus = "-";
            if (client.State.GetType().Name == "ConnectedState")
            {
                portStatus = client.IsPortOpened ? "開放" : "未開放";
            }

            var hasValidKey =
                client.Key != null && !client.Key.IsExpired(client.CalcNowProtocolTime());

            App.Current.Dispatcher.Invoke(() =>
            {
                viewModel.Status = status;
                viewModel.StatusIconGlyph = statusIconGlyph;
                viewModel.PortStatus = $"ポート: {portStatus}";
                viewModel.CanSendUserquake = hasValidKey;

                viewModel.StatusViewModel.PortStatus = portStatus;
                viewModel.StatusViewModel.KeyStatus = hasValidKey ? "有効" : "無効";
                viewModel.StatusViewModel.StatusTitle = statusTitle;
                viewModel.StatusViewModel.StatusDescription = statusDescription;
            });
        }

        private static string ChoiceByState(string disconnected, string disconnecting, string connecting, string connected, string noConnection)
        {
            return client.State.GetType().Name switch
            {
                "DisconnectedState" => disconnected,
                "DisconnectingState" => disconnecting,
                "ConnectingState" => connecting,
                _ => client.Connections > 0 ? connected : noConnection
            };
        }
    }
}
