using Client.App;
using Client.Client;
using EPSPWPFClient.Controls;
using EPSPWPFClient.ViewModel;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EPSPWPFClient.Mediator
{
    class EPSPMediator
    {
        IMediatorContext mediatorContext;
        internal StatusViewModel StatusViewModel { get; private set; }
        internal HistoryViewModel HistoryViewModel { get; private set; }
        internal EPSPHandler epspHandler { get; private set; }

        public EPSPMediator()
        {
            // Model
            mediatorContext = new MediatorContext();
            epspHandler = new EPSPHandler();

            mediatorContext.ConnectionsChanged += MediatorContext_ConnectionsChanged;
            mediatorContext.StateChanged += MediatorContext_StateChanged;
            mediatorContext.Completed += MediatorContext_Completed;

            //mediatorContext.OnAreapeers += epspHandler.MediatorContext_OnAreapeers;
            mediatorContext.OnEarthquake += epspHandler.MediatorContext_OnEarthquake;
            //mediatorContext.OnTsunami += epspHandler.MediatorContext_OnTsunami;
            //mediatorContext.OnEEWTest += epspHandler.MediatorContext_OnEEWTest;
            //mediatorContext.OnUserquake += epspHandler.MediatorContext_OnUserquake;

            // ViewModel <=> Model
            StatusViewModel = new StatusViewModel();
            StatusViewModel.CanConnect.Value = mediatorContext.CanConnect;
            StatusViewModel.CanDisconnect.Value = mediatorContext.CanDisconnect;
            StatusViewModel.ConnectCommand.Subscribe(() =>
            {
                mediatorContext.Connect();
            });
            StatusViewModel.DisconnectCommand.Subscribe(async () =>
            {
                await Task.Run(() => mediatorContext.Disconnect());
            });

            HistoryViewModel = new HistoryViewModel(epspHandler);
        }

        private void MediatorContext_Completed(object sender, OperationCompletedEventArgs e)
        {
            StatusViewModel.ConnectionStatus.Value = mediatorContext.State.GetType().Name;
            StatusViewModel.NumberOfPeers.Value = mediatorContext.PeerCount;
            StatusViewModel.IsKeyAllocated.Value = (mediatorContext.Key != null && !mediatorContext.Key.IsExpired(mediatorContext.CalcNowProtocolTime()));
            StatusViewModel.IsPortOpened.Value = mediatorContext.IsPortOpened;
        }

        private void MediatorContext_StateChanged(object sender, EventArgs e)
        {
            StatusViewModel.ConnectionStatus.Value = mediatorContext.State.GetType().Name;
            Application.Current.Dispatcher.Invoke(() =>
            {
                StatusViewModel.CanConnect.Value = mediatorContext.CanConnect;
                StatusViewModel.CanDisconnect.Value = mediatorContext.CanDisconnect;
            });
        }

        private void MediatorContext_ConnectionsChanged(object sender, EventArgs e)
        {
            StatusViewModel.Connections.Value = mediatorContext.Connections;
        }

        public void Start()
        {
            mediatorContext.Connect();
        }

        public void Stop()
        {
            mediatorContext.Disconnect();
        }
    }
}
