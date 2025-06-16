using AvaloniaUIClient.ViewModels;
using AvaloniaUIClient.ViewModels.Information;

using Client.App;
using Client.App.Userquake;
using Client.Peer;

using System.Diagnostics;
using System.Threading;

namespace AvaloniaUIClient.Mediator
{
    public class Mediator
    {
        private readonly MediatorContext client;
        private readonly MainWindowViewModel viewModel;

        public MediatorContext MediatorContext { get { return this.client; } }

        public Mediator(MainWindowViewModel viewModel)
        {
            client = new MediatorContext();
            client.ConnectionsChanged += Client_ConnectionsChanged;
            client.StateChanged += Client_StateChanged;
            client.OnTsunami += Client_OnTsunami;
            client.OnEarthquake += Client_OnEarthquake;
            client.OnEEW += Client_OnEEW;
            client.OnNewUserquakeEvaluation += Client_OnNewUserquakeEvaluation;
            this.viewModel = viewModel;
        }

        public void Start()
        {
            client.Connect();
        }

        public void Stop()
        {
            if (!client.CanDisconnect)
            {
                return;
            }

            var sw = new Stopwatch();
            sw.Start();
            client.Disconnect();

            while (sw.ElapsedMilliseconds <= 4000 && !client.CanConnect)
            {
                Thread.Sleep(250);
            }
        }

        private void Client_StateChanged(object? sender, System.EventArgs e)
        {
            updateStateLabel();
        }

        private void Client_ConnectionsChanged(object? sender, System.EventArgs e)
        {
            updateStateLabel();
        }

        private void updateStateLabel()
        {
            var state = client.ReadonlyState;
            var stateLabel = state.GetType().Name switch
            {
                "DisconnectedState" => "未接続",
                "DisconnectingState" => "切断中...",
                "ConnectingState" => "接続中...",
                _ => client.Connections > 0 ? "接続済み" : "接続なし"
            };

            viewModel.Status = stateLabel;

            if (state.GetType().Name == "ConnectedState")
            {
                viewModel.PortStatus = client.IsPortOpened ? "ポート: 開放" : "ポート: 未開放";
            }
            else
            {
                viewModel.PortStatus = "ポート: -";
            }
        }

        private void Client_OnTsunami(object? sender, EPSPTsunamiEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"津波イベント受信: IsCancelled={e.IsCancelled}, RegionList Count={e.RegionList?.Count ?? 0}");
            
            var tsunamiViewModel = new TsunamiViewModel();
            System.Diagnostics.Debug.WriteLine($"TsunamiViewModel作成完了: {tsunamiViewModel.GetType().FullName}");
            
            tsunamiViewModel.Initialize(e);
            System.Diagnostics.Debug.WriteLine("TsunamiViewModel初期化完了");

            viewModel.InformationViewModel.Histories.Insert(0, e);
            viewModel.InformationViewModel.ActiveEventArgs = e;
            viewModel.InformationViewModel.ActiveViewModel = tsunamiViewModel;
            
            System.Diagnostics.Debug.WriteLine($"ActiveViewModel設定完了: {viewModel.InformationViewModel.ActiveViewModel?.GetType().FullName}");
        }

        private void Client_OnEarthquake(object? sender, EPSPQuakeEventArgs e)
        {
            var earthquakeViewModel = new EarthquakeViewModel(e, viewModel.InformationViewModel);

            viewModel.InformationViewModel.Histories.Insert(0, e);
            viewModel.InformationViewModel.ActiveEventArgs = e;
            viewModel.InformationViewModel.ActiveViewModel = earthquakeViewModel;
        }

        private void Client_OnEEW(object? sender, EPSPEEWEventArgs e)
        {
            var eewViewModel = new EEWViewModel(e, viewModel.InformationViewModel);

            viewModel.InformationViewModel.Histories.Insert(0, e);
            viewModel.InformationViewModel.ActiveEventArgs = e;
            viewModel.InformationViewModel.ActiveViewModel = eewViewModel;
        }

        private void Client_OnNewUserquakeEvaluation(object? sender, UserquakeEvaluateEventArgs e)
        {
            var userquakeViewModel = new UserquakeViewModel(e, viewModel.InformationViewModel);

            viewModel.InformationViewModel.Histories.Insert(0, e);
            viewModel.InformationViewModel.ActiveEventArgs = e;
            viewModel.InformationViewModel.ActiveViewModel = userquakeViewModel;
        }
    }
}
