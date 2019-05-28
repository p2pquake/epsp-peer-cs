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
        private ClientConfiguration configuration;

        public ShowHandler()
        {
            configuration = ClientConfiguration.Instance;
        }

        private void Show(EPSPDataEventArgs e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                var mainWindow = (MainWindow)App.Current.MainWindow;
                if (mainWindow.Visibility != System.Windows.Visibility.Visible)
                {
                    mainWindow.Show();
                    // XXX: あまり良い方法ではない
                    mainWindow.menuListBox.SelectedIndex = -1;
                    mainWindow.menuListBox.SelectedIndex = 0;
                    mainWindow.HistoryControl.comboBox.SelectedIndex = -1;
                    mainWindow.HistoryControl.comboBox.SelectedIndex = 0;
                }
                if (mainWindow.WindowState == System.Windows.WindowState.Minimized)
                {
                    mainWindow.WindowState = System.Windows.WindowState.Normal;
                }
            }); 
        }

        public void OnAreapeers(EventArgs e)
        {
            // noop
        }

        public void OnEarthquake(EPSPQuakeEventArgs e)
        {
            if (!configuration.Show.IsEarthquake.Value ||
                 ConvertScale(e.Scale) < configuration.Show.EarthquakeSeismicScale)
            {
                return;
            }

            Show(e);
        }

        public void OnEEWTest(EPSPEEWTestEventArgs e)
        {
            if (!configuration.Show.IsEEWTest)
            {
                return;
            }

            Show(e);
        }

        public void OnTsunami(EPSPTsunamiEventArgs e)
        {
            if (!configuration.Show.IsTsunami)
            {
                return;
            }

            Show(e);
        }

        public void OnUserquake(EPSPUserquakeEventArgs e)
        {
            // noop
        }

        public void OnUserquakeReached(EPSPUQSummaryEventArgs e)
        {
            // TODO: 信頼度等のしきい値設定については未実装
            if (!configuration.Show.IsUserquake)
            {
                return;
            }

            Show(e);
        }

        public void OnUserquakeUpdated(EPSPUQSummaryEventArgs e)
        {
            // noop
        }

        private int ConvertScale(string scale)
        {
            var scaleDictionary = new Dictionary<string, int>()
            {
                { "1", 10 },
                { "2", 20 },
                { "3", 30 },
                { "4", 40 },
                { "5弱", 45 },
                { "5強", 50 },
                { "6弱", 55 },
                { "6強", 60 },
                { "7", 70 }
            };

            if (scaleDictionary.ContainsKey(scale))
            {
                return scaleDictionary[scale];
            }

            return -1;
        }
    }
}
