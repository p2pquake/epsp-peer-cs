using Updater;

using Sentry;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

using static Updater.UpdateClient;

namespace AutoUpdater
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer;
        private int closeCount;

        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;
        }

        private void TreatException(Exception ex)
        {
            var dataContext = (MainWindowModel)DataContext;

            dataContext.UpdatedResultMessage = $"アップデートに失敗しました。\n\nエラー: {ex.Message}";
            dataContext.UpdateStatus = UpdateStatus.Updated;

            if (Program.silent)
            {
                closeCount = 30;
                timer = new DispatcherTimer() {  Interval = TimeSpan.FromSeconds(1) };
                timer.Tick += (s, e) =>
                {
                    closeCount--;
                    dataContext.CloseButtonContent = $"閉じる ({closeCount} 秒後に自動的に閉じます)";
                    if (closeCount <= 0)
                    {
                        timer.Stop();
                        this.Close();
                    }
                };
                timer.Start();
                return;
            }
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var dataContext = (MainWindowModel)DataContext;

            bool hasUpdate;
            try
            {
                hasUpdate = (await UpdateClient.Check()).Length > 0;
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                TreatException(ex);
                return;
            }

            if (hasUpdate)
            {
                dataContext.UpdateStatus = UpdateStatus.Confirmation;

                if (Program.silent)
                {
                    UpdateButton_Click(this, e);
                }
                return;
            }

            if (Program.silent)
            {
                this.Close();
                return;
            }

            dataContext.UpdatedResultMessage = "アップデートはありません。最新の状態です。";
            dataContext.UpdateStatus = UpdateStatus.Updated;
        }

        private async void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            var dataContext = (MainWindowModel)DataContext;
            dataContext.UpdateStatus = UpdateStatus.Updating;

            UpdateResult result;
            var terminateResult = P2PQuakeCommunicator.TerminateP2PQuake();
            try
            {
                var updates = await Task.Run(async () =>
                {
                    return await UpdateClient.Check();
                });
                result = await Task.Run(async () =>
                {
                    return await UpdateClient.Update(updates);
                });
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                TreatException(ex);
                if (terminateResult == P2PQuakeCommunicator.TerminateResult.Terminated)
                {
                    P2PQuakeCommunicator.BootP2PQuake();
                }
                return;
            }

            switch (result)
            {
                case UpdateClient.UpdateResult.Failure:
                    dataContext.UpdatedResultMessage = $"アップデートに失敗しました。";
                    break;
                case UpdateClient.UpdateResult.Success:
                    if (terminateResult == P2PQuakeCommunicator.TerminateResult.Terminated)
                    {
                        P2PQuakeCommunicator.BootP2PQuake();
                        Close();
                        return;
                    }
                    dataContext.UpdatedResultMessage = "アップデートが完了しました。";
                    break;
                default:
                    break;
            }

            dataContext.UpdateStatus = UpdateStatus.Updated;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
