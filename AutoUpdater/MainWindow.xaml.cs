using AutoUpdater.Updater;

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

namespace AutoUpdater
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var dataContext = (MainWindowModel)DataContext;

            var updates = await UpdateClient.Check();
            if (updates.Length > 0)
            {
                dataContext.UpdateStatus = UpdateStatus.Confirmation;
                return;
            }

            // FIXME: 自動更新モードの場合、アップデートなければ静かに終了する
            //if (quitIfNoUpdate)
            //{
            //    this.Close();
            //    return;
            //}

            dataContext.UpdatedResultMessage = "アップデートはありません。最新の状態です。";
            dataContext.UpdateStatus = UpdateStatus.Updated;
        }

        private async void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            var dataContext = (MainWindowModel)DataContext;
            dataContext.UpdateStatus = UpdateStatus.Updating;

            var updates = await Task.Run(async () =>
            {
                return await UpdateClient.Check();
            });

            var result = await Task.Run(async () =>
            {
                return await UpdateClient.Update(updates);
            });
            switch (result)
            {
                case UpdateClient.UpdateResult.SuccessAndRestart:
                    Close();
                    return;
                case UpdateClient.UpdateResult.Failure:
                    dataContext.UpdatedResultMessage = $"アップデートに失敗しました。";
                    break;
                case UpdateClient.UpdateResult.Success:
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
