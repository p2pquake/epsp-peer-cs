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
        /// <summary>更新がないときに終了する</summary>
        public static bool silent = false;

        [STAThread]
        public static void Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "silent")
            {
                silent = true;
            }

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
