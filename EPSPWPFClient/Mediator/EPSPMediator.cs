using Client.App;
using Client.Client;
using EPSPWPFClient.Controls;
using EPSPWPFClient.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPSPWPFClient.Mediator
{
    class EPSPMediator
    {
        IMediatorContext mediatorContext = new MediatorContext();
        internal StatusViewModel StatusViewModel { get; } = new StatusViewModel();

        public EPSPMediator()
        {
            mediatorContext = new MediatorContext();
            //var epspHandler = new EPSPHandler(mediatorContext.CalcNowProtocolTime);

            mediatorContext.ConnectionsChanged += MediatorContext_ConnectionsChanged;
            mediatorContext.StateChanged += MediatorContext_StateChanged;
            mediatorContext.Completed += MediatorContext_Completed;

            //mediatorContext.OnAreapeers += epspHandler.MediatorContext_OnAreapeers;
            //mediatorContext.OnEarthquake += epspHandler.MediatorContext_OnEarthquake;
            //mediatorContext.OnTsunami += epspHandler.MediatorContext_OnTsunami;
            //mediatorContext.OnEEWTest += epspHandler.MediatorContext_OnEEWTest;
            //mediatorContext.OnUserquake += epspHandler.MediatorContext_OnUserquake;
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
