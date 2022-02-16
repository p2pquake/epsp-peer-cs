using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AutoUpdater
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            App app = new();
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            _ = app.Run();
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var dataContext = (MainWindowModel)App.Current.MainWindow.DataContext;
            if (dataContext != null)
            {
                dataContext.UpdatedResultMessage = "アップデートに失敗しました。";
                dataContext.UpdateStatus = UpdateStatus.Updated;
            }

            _ = MessageBox.Show($"エラーが発生したため、アップデートを中断しました。\n\nエラー: {((Exception)e.ExceptionObject).Message}", "P2P地震情報 アップデーター", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
