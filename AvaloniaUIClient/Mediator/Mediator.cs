using AvaloniaUIClient.ViewModels;

using Client.App;

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
    }
}
