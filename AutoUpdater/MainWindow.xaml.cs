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
        }

        private async void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            var dataContext = (MainWindowModel)App.Current.MainWindow.DataContext;
            dataContext.UpdateMessage = "アップデートしています...";
            dataContext.UpdateStatus = UpdateStatus.Updating;

            var updates = await Task.Run(async () =>
            {
                return await UpdateClient.CheckUpdateAsync();
            });

            dataContext.UpdateMessage = $"アップデートしています... (全 {updates.Length} ファイル)";

            var result = await Task.Run(async () =>
            {
                return await UpdateClient.UpdateAsync(updates);
            });

            switch (result)
            {
                case UpdateClient.UpdateResult.SuccessAndRestart:
                    Close();
                    return;
                case UpdateClient.UpdateResult.Failure:
                    // FIXME: このメッセージは適切でない。ネットワークエラーかもしれない。
                    dataContext.UpdateMessage = "アップデートに失敗しました。\nアプリやファイルを閉じていること、書き込み権限があることを確認してください。";
                    break;
                case UpdateClient.UpdateResult.Success:
                    dataContext.UpdateMessage = "アップデートが完了しました。";
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
