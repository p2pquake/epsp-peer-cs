using Client.App;
using Client.Peer;

using Polly;

using Sentry;

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
            WithRetry(() =>
                {
                    App.Current.Dispatcher.Invoke(() =>
                                {
                                    var dataContext = (RootViewModel)App.Current.MainWindow.DataContext;
                                    var item = dataContext.InformationViewModel.Histories.First((item) =>
                                                        (type == "quake" && item is EPSPQuakeView quake && quake.EventArgs.ReceivedAt.ToString() == receivedAt) ||
                                                        (type == "tsunami" && item is EPSPTsunamiView tsunami && tsunami.EventArgs.ReceivedAt.ToString() == receivedAt) ||
                                                        (type == "eew" && item is EPSPEEWView eew && eew.EventArgs.ReceivedAt.ToString() == receivedAt) ||
                                                        (type == "userquake" && item is EPSPUserquakeView userquake && userquake.EventArgs.StartedAt.ToString() == startedAt)
                                                    );
                                    dataContext.InformationViewModel.SelectedIndex = dataContext.InformationViewModel.Histories.IndexOf(item);
                                    dataContext.InformationIsSelected = true;
                                });
                });
        }

        public static void Activate(string type, string receivedAt = null, string startedAt = null)
        {
            Select(type, receivedAt, startedAt);
            App.Current.Dispatcher.Invoke(() =>
            {
                var window = App.Current.MainWindow;
                window.Show();
                if (window.WindowState == System.Windows.WindowState.Minimized)
                {
                    window.WindowState = System.Windows.WindowState.Normal;
                }
                window.Activate();
            });
        }

        public Activator(Configuration configuration, MediatorContext mediatorContext)
        {
            this.configuration = configuration;

            mediatorContext.OnEarthquake += (s, e) => { Task.Run(() => MediatorContext_OnEarthquake(s, e)); };
            mediatorContext.OnTsunami += (s, e) => { Task.Run(() => MediatorContext_OnTsunami(s, e)); };
            mediatorContext.OnEEW += (s, e) => { Task.Run(() => MediatorContext_OnEEW(s, e)); };
            mediatorContext.OnNewUserquakeEvaluation += (s, e) => { Task.Run(() => MediatorContext_OnNewUserquakeEvaluation(s, e)); };
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
            var foreign = e.InformationType == QuakeInformationType.Foreign && earthquakeNotification.Foreign;
            if (scale < earthquakeNotification.MinScale && !foreign)
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

        private void MediatorContext_OnEEW(object sender, EPSPEEWEventArgs e)
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
        private static void WithRetry(Action a)
        {
            try
            {
                Policy.Handle<Exception>().WaitAndRetry(8, retryAttempt => TimeSpan.FromSeconds(Math.Pow(1.2, retryAttempt - 1) - 0.5)).Execute(a);
            } catch (Exception e)
            {
                SentrySdk.CaptureException(e);
            }
        }
    }
}
