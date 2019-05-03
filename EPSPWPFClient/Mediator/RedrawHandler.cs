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
            // TODO: ピア分布図を更新
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
            Redraw();
        }

        public void OnUserquakeUpdated(EPSPUQSummaryEventArgs e)
        {
            Redraw();
        }

        private void Redraw()
        {
            // XXX TODO: 依存の仕方がよくない
            App.Current.Dispatcher.Invoke(() =>
            {
                ((HistoryViewModel)((MainWindow)App.Current.MainWindow).HistoryControl.DataContext)?.RedrawCommand?.Execute();
            });
        }
    }
}
