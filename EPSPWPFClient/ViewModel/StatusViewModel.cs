using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EPSPWPFClient.ViewModel
{
    class StatusViewModel
    {
        public ReactiveCommand ConnectCommand { get; private set; } // = new ReactiveCommand();
        public AsyncReactiveCommand DisconnectCommand { get; private set; } // = new AsyncReactiveCommand();

        public ReactiveCommand ShowCommand { get; private set; } = new ReactiveCommand();
        public AsyncReactiveCommand ExitCommand { get; private set; } = new AsyncReactiveCommand();

        public AsyncReactiveCommand RightDoubleClickCommand { get; private set; } = new AsyncReactiveCommand();
        public AsyncReactiveCommand MiddleDoubleClickCommand { get; private set; } = new AsyncReactiveCommand();

        public ReactiveProperty<bool> CanConnect { get; } = new ReactiveProperty<bool>();
        public ReactiveProperty<bool> CanDisconnect { get; } = new ReactiveProperty<bool>();

        public ReactiveProperty<string> ConnectionStatus { get; } = new ReactiveProperty<string>("");

        public ReactiveProperty<int> Connections { get; } = new ReactiveProperty<int>(0);
        public ReactiveProperty<int> NumberOfPeers { get; } = new ReactiveProperty<int>(0);

        public ReactiveProperty<bool> IsPortOpened { get; } = new ReactiveProperty<bool>();
        public ReactiveProperty<bool> IsKeyAllocated { get; } = new ReactiveProperty<bool>();

        public StatusViewModel()
        {
            ConnectCommand = CanConnect.Select(x => x).ToReactiveCommand();
            DisconnectCommand = CanDisconnect.Select(x => x).ToAsyncReactiveCommand();

            ShowCommand.Subscribe((e) =>
            {
                var window = Application.Current.MainWindow;
                window.Show();
                if (window.WindowState == WindowState.Minimized)
                {
                    window.WindowState = WindowState.Normal;
                }
            });

            ExitCommand.Subscribe(async _ =>
            {
                if (CanDisconnect.Value)
                {
                    DisconnectCommand.Execute();
                    foreach (var i in Enumerable.Range(0, 6))
                    {
                        await Task.Delay(1000);
                        if (CanConnect.Value)
                        {
                            break;
                        }
                    }
                }
                Application.Current.Shutdown();
            });
        }
    }
}
