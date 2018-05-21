using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPSPWPFClient.ViewModel
{
    class StatusViewModel
    {
        public ReactiveCommand ConnectCommand { get; } = new ReactiveCommand();
        public AsyncReactiveCommand DisconnectCommand { get; } = new AsyncReactiveCommand();

        public ReactiveProperty<string> ConnectionStatus { get; } = new ReactiveProperty<string>("");

        public ReactiveProperty<int> Connections { get; } = new ReactiveProperty<int>(0);
        public ReactiveProperty<int> NumberOfPeers { get; } = new ReactiveProperty<int>(0);

        public ReactiveProperty<bool> IsPortOpened { get; } = new ReactiveProperty<bool>();
        public ReactiveProperty<bool> IsKeyAllocated { get; } = new ReactiveProperty<bool>();
    }
}
