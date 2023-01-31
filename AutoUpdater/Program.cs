using Sentry;

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

            InitSentry();

            App app = new();
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            _ = app.Run();
        }

        private static void InitSentry()
        {
            SentrySdk.Init(o =>
            {
                o.Dsn = "https://812c520eddb245a69b331b802770c513@o1151228.ingest.sentry.io/6228133";
#if DEBUG
                o.Environment = "debug";
#else
                o.Environment = "release";
#endif
            });
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            SentrySdk.CaptureException((Exception)e.ExceptionObject);

            var dataContext = (MainWindowModel)App.Current.MainWindow.DataContext;
            if (dataContext != null)
            {
                dataContext.UpdatedResultMessage = "アップデートに失敗しました。";
                dataContext.UpdateStatus = UpdateStatus.Updated;
            }

            if (silent)
            {
                dataContext.UpdatedResultMessage = $"アップデートに失敗しました。\n\nエラー: {((Exception)e.ExceptionObject).Message}";
                return;
            }

            _ = MessageBox.Show($"エラーが発生したため、アップデートを中断しました。\n\nエラー: {((Exception)e.ExceptionObject).Message}", "P2P地震情報 アップデーター", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
