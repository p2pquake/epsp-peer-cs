using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.Peer;
using EPSPWPFClient.Userquake;
using EPSPWPFClient.ViewModel;

namespace EPSPWPFClient.Mediator
{
    class RedrawHandler : IEPSPHandleable
    {
        public void OnAreapeers(EPSPAreapeersEventArgs e)
        {
            RedrawPeerMap();
        }

        public void OnEarthquake(EPSPQuakeEventArgs e)
        {
            // noop
        }

        public void OnEEWTest(EPSPEEWTestEventArgs e)
        {
            // noop
        }

        public void OnTsunami(EPSPTsunamiEventArgs e)
        {
            // noop
        }

        public void OnUserquake(EPSPUserquakeEventArgs e)
        {
            // noop (use OnUserquakeReached and OnUserquakeUpdated)
        }

        public void OnUserquakeReached(EPSPUQSummaryEventArgs e)
        {
            RedrawHistory();
        }

        public void OnUserquakeUpdated(EPSPUQSummaryEventArgs e)
        {
            RedrawHistory();
        }

        private void RedrawPeerMap()
        {
            // XXX TODO: 依存の仕方がよくない
            App.Current.Dispatcher.Invoke(() =>
            {
                ((PeerMapViewModel)((MainWindow)App.Current.MainWindow).PeerMapControl.DataContext)?.RedrawCommand?.Execute();
            });
        }

        private void RedrawHistory()
        {
            // XXX TODO: 依存の仕方がよくない
            App.Current.Dispatcher.Invoke(() =>
            {
                ((HistoryViewModel)((MainWindow)App.Current.MainWindow).HistoryControl.DataContext)?.RedrawCommand?.Execute();
            });
        }
    }
}
