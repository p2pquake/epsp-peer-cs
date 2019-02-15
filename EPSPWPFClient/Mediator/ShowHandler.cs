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
    class ShowHandler : IEPSPHandleable
    {
        private void Show(EPSPDataEventArgs e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                var mainWindow = (MainWindow)App.Current.MainWindow;
                if (mainWindow.Visibility != System.Windows.Visibility.Visible)
                {
                    // FIXME: 履歴の最新情報を表示してほしい。
                    mainWindow.Show();
                }
                if (mainWindow.WindowState == System.Windows.WindowState.Minimized)
                {
                    mainWindow.WindowState = System.Windows.WindowState.Normal;
                }
            }); 
        }

        public void OnAreapeers(EPSPAreapeersEventArgs e)
        {
            Show(e);
        }

        public void OnEarthquake(EPSPQuakeEventArgs e)
        {
            Show(e);
        }

        public void OnEEWTest(EPSPEEWTestEventArgs e)
        {
            Show(e);
        }

        public void OnTsunami(EPSPTsunamiEventArgs e)
        {
            Show(e);
        }

        public void OnUserquake(EPSPUserquakeEventArgs e)
        {
            // noop
        }

        public void OnUserquakeReached(EPSPUQSummaryEventArgs e)
        {
            Show(e);
        }

        public void OnUserquakeUpdated(EPSPUQSummaryEventArgs e)
        {
            // noop
        }
    }
}
