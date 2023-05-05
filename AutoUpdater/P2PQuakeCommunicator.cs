using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AutoUpdater
{
    public class P2PQuakeCommunicator
    {
        public enum TerminateResult
        {
            NotRunning,
            Terminated,
        }

        public static TerminateResult TerminateP2PQuake()
        {
            // プロセスは起動してる？
            var processes = Process.GetProcessesByName("P2PQuake");
            if (processes.Length <= 0)
            {
                return TerminateResult.NotRunning;
            }

            // パイプを開き、終了してもらう
            using var pipe = new NamedPipeClientStream(".", IPC.Const.Name, PipeDirection.Out, PipeOptions.CurrentUserOnly);
            try
            {
                pipe.Connect(1000);
                using var stream = new StreamWriter(pipe);
                stream.AutoFlush = true;
                stream.WriteLine(JsonSerializer.Serialize(new IPC.Message(IPC.Method.Exit)));
            }
            catch (TimeoutException)
            {
                // 古いバージョンの場合はここに来る。強制終了する
                processes.ToList().ForEach(process => process.Kill());
            }

            // 終了まで 10 秒待つ
            var sw = new Stopwatch();
            sw.Start();

            while (sw.ElapsedMilliseconds <= 10000)
            {
                if (processes.All(process => { process.Refresh(); return process.HasExited; })) { return TerminateResult.Terminated; }
                Thread.Sleep(500);
            }

            throw new Exception("P2P地震情報を終了できませんでした。");
        }

        public static void BootP2PQuake() {
            Process.Start(GeneratePath("P2PQuake.exe"), "noautoupdate");
        }

        private static string GeneratePath(string path)
        {
            return Path.Join(GetAppDirectory(), path.Replace('/', Path.DirectorySeparatorChar));
        }

        private static string GetAppDirectory()
        {
            return Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
        }
    }
}
