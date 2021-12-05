using Client.App;
using Client.Peer;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WpfClient.EPSPDataView;
using WpfClient.Utils;

namespace WpfClient.Notifications
{
    public class Activator
    {
        private Configuration configuration;

        public static void Select(string type, string receivedAt = null, string startedAt = null)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                var dataContext = (RootViewModel)App.Current.MainWindow.DataContext;
                var item = dataContext.InformationViewModel.Histories.First((item) =>
                    (type == "quake" && item is EPSPQuakeView quake && quake.EventArgs.ReceivedAt.ToString() == receivedAt) ||
                    (type == "tsunami" && item is EPSPTsunamiView tsunami && tsunami.EventArgs.ReceivedAt.ToString() == receivedAt) ||
                    (type == "eew" && item is EPSPEEWTestView eew && eew.EventArgs.ReceivedAt.ToString() == receivedAt) ||
                    (type == "userquake" && item is EPSPUserquakeView userquake && userquake.EventArgs.StartedAt.ToString() == startedAt)
                );
                dataContext.InformationViewModel.SelectedIndex = dataContext.InformationViewModel.Histories.IndexOf(item);
                dataContext.InformationIsSelected = true;
            });
        }

        public static void Activate(string type, string receivedAt = null, string startedAt = null)
        {
            Select(type, receivedAt, startedAt);
            App.Current.Dispatcher.Invoke(() =>
            {
                App.Current.MainWindow.Show();
                App.Current.MainWindow.Activate();
            });
        }

        public Activator(Configuration configuration, MediatorContext mediatorContext)
        {
            this.configuration = configuration;

            mediatorContext.OnEarthquake += MediatorContext_OnEarthquake;
            mediatorContext.OnTsunami += MediatorContext_OnTsunami;
            mediatorContext.OnEEWTest += MediatorContext_OnEEWTest;
            mediatorContext.OnNewUserquakeEvaluation += MediatorContext_OnNewUserquakeEvaluation;
        }

        private void MediatorContext_OnEarthquake(object sender, EPSPQuakeEventArgs e)
        {
            if (e.InformationType == QuakeInformationType.Unknown)
            {
                return;
            }

            var earthquakeNotification = configuration.EarthquakeNotification;
            if (!earthquakeNotification.Enabled)
            {
                return;
            }

            var scale = e.InformationType == QuakeInformationType.Destination ? 30 : ScaleConverter.Str2Int(e.Scale);
            if (scale < earthquakeNotification.MinScale)
            {
                return;
            }

            if (!earthquakeNotification.Show)
            {
                Select("quake", e.ReceivedAt.ToString());
                return;
            }
            Activate("quake", e.ReceivedAt.ToString());
        } 

        private void MediatorContext_OnTsunami(object sender, EPSPTsunamiEventArgs e)
        {
            var tsunamiNotification = configuration.TsunamiNotification;

            if (!tsunamiNotification.Enabled)
            {
                return;
            }

            if (!tsunamiNotification.Show)
            {
                Select("tsunami", e.ReceivedAt.ToString());
                return;
            }
            Activate("tsunami", e.ReceivedAt.ToString());
        }

        private void MediatorContext_OnEEWTest(object sender, EPSPEEWTestEventArgs e)
        {
            var eewTestNotification = configuration.EEWTestNotification;

            if (!eewTestNotification.Enabled)
            {
                return;
            }

            if (!eewTestNotification.Show)
            {
                Select("eew", e.ReceivedAt.ToString());
                return;
            }
            Activate("eew", e.ReceivedAt.ToString());
        }

        private void MediatorContext_OnNewUserquakeEvaluation(object sender, Client.App.Userquake.UserquakeEvaluateEventArgs e)
        {
            var userquakeNotification = configuration.UserquakeNotification;

            if (!userquakeNotification.Enabled)
            {
                return;
            }

            if (!userquakeNotification.Show)
            {
                Select("userquake", null, e.StartedAt.ToString());
                return;
            }
            Activate("userquake", null, e.StartedAt.ToString());
        }
    }
}
