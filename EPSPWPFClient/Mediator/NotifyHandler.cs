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
    class NotifyHandler : IEPSPHandleable
    {
        private void Notify(EPSPDataEventArgs e)
        {
            var title = EPSPTitleConverter.GetTitle(e);
            if (title == "") { return; }

            App.Current.Dispatcher.Invoke(() =>
            {
                ((MainWindow)App.Current.MainWindow).epspNotifyIcon.ShowBalloonTip("P2P地震情報", title, Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);
            }); 
        }

        public void OnAreapeers(EPSPAreapeersEventArgs e)
        {
            Notify(e);
        }

        public void OnEarthquake(EPSPQuakeEventArgs e)
        {
            Notify(e);
        }

        public void OnEEWTest(EPSPEEWTestEventArgs e)
        {
            Notify(e);
        }

        public void OnTsunami(EPSPTsunamiEventArgs e)
        {
            Notify(e);
        }

        public void OnUserquake(EPSPUserquakeEventArgs e)
        {
            // noop
        }

        public void OnUserquakeReached(EPSPUQSummaryEventArgs e)
        {
            Notify(e);
        }

        public void OnUserquakeUpdated(EPSPUQSummaryEventArgs e)
        {
            // noop
        }
    }
}
