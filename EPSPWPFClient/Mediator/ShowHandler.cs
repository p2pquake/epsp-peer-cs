using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.Peer;
using EPSPWPFClient.ViewModel;

namespace EPSPWPFClient.Mediator
{
    class ShowHandler : IEPSPHandleable
    {
        private void Notify(EPSPDataEventArgs e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                var mainWindow = (MainWindow)App.Current.MainWindow;
                if (mainWindow.Visibility != System.Windows.Visibility.Visible)
                {
                    mainWindow.Show();
                }
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
            Notify(e);
        }
    }
}
