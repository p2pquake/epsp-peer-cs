using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WpfClient
{
    public static class Updater
    {
        private static readonly string updaterFilename = "P2PQuakeAutoUpdater.exe";
        private static readonly string updaterSourceFilename = "P2PQuakeAutoUpdater2.exe";

        public static void Run()
        {
            string path = GeneratePath(updaterFilename);
            if (!File.Exists(path)) {
                // TODO: 存在しないことを伝えたほうがいいかも。
                return;
            }
            var process = Process.Start(path, "silent");
            process.WaitForExit();
        }

        /// <summary>AutoUpdater の更新処理</summary>
        public static void UpdateUpdater()
        {
            string sourcePath = GeneratePath(updaterSourceFilename);
            string destPath = GeneratePath(updaterFilename);
            if (!File.Exists(sourcePath))
            {
                return;
            }

            // AutoUpdater の終了を待つのが本来の姿だが、
            // 手抜きで成功するまで何度かリトライする
            for (int i = 0; i < 10; i++)
            {
                // 1, 2, 4, 8, 16, 32, 60, ...
                Thread.Sleep(Math.Min(60000, (int)Math.Pow(2, i) * 1000)); // exponential backoff
                try
                {
                    File.Move(sourcePath, destPath, true);
                    break;
                } catch (UnauthorizedAccessException) {
                    // noop.
                }
            }
        }

        public static bool HasUpdaterUpdate()
        {
            string sourcePath = GeneratePath(updaterSourceFilename);
            return File.Exists(sourcePath);
        }

        private static string GetAppDirectory()
        {
            return Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
        }

        private static string GeneratePath(string path)
        {
            return Path.Join(GetAppDirectory(), path.Replace('/', Path.DirectorySeparatorChar));
        }
    }
}
