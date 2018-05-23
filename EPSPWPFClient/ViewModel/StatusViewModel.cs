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
        }
    }
}
