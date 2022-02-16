using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AutoUpdater.Updater
{
    public class UpdateClient
    {
        public enum UpdateResult
        {
            Failure,
            Success,
            SuccessAndRestart,
        }

        enum TerminateResult
        {
            NotRunning,
            Terminated,
        }

        private static string UpdateUri = "https://www.p2pquake.net/update";
        private static HttpClient Client = new HttpClient() { Timeout = TimeSpan.FromSeconds(30) };

        public static async Task<UpdateEntry[]> Check()
        {
            var response = await Client.GetStringAsync($"{UpdateUri}/update.json");
            var entries = JsonSerializer.Deserialize<UpdateEntry[]>(response);

            return entries.Where(entry =>
            {
                var path = GeneratePath(entry);
                if (entry.required && !File.Exists(path))
                {
                    return true;
                }

                if (!entry.allowDigestMismatch && File.Exists(path) && CalcSha256(path) != entry.sha256Digest)
                {
                    return true;
                }

                return false;
            }).ToArray();
        }

        public static async Task<UpdateResult> Update(UpdateEntry[] entries)
        {
            var terminateResult = TerminateP2PQuake();

            foreach (var entry in entries)
            {
                var response = await Client.GetStreamAsync($"{UpdateUri}/{entry.path}");
                var path = GeneratePath(entry);
                var directory = Path.GetDirectoryName(path);

                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using var fs = new FileStream($"{path}.tmp", FileMode.OpenOrCreate, FileAccess.Write);
                response.CopyTo(fs);
                fs.Close();

                if (CalcSha256($"{path}.tmp") != entry.sha256Digest)
                {
                    throw new Exception($"ダウンロードしたファイル {entry.path} が壊れています。");
                }

                // ファイルが存在する場合、 ReplaceFile API で極力アトミックに差し替える
                if (File.Exists(path))
                {
                    File.Replace($"{path}.tmp", path, null);
                } else
                {
                    File.Move($"{path}.tmp", path);
                }
            }

            if (terminateResult == TerminateResult.Terminated)
            {
                Process.Start(GeneratePath("P2PQuake.exe"));
                return UpdateResult.SuccessAndRestart;
            }

            return UpdateResult.Success;
        }

        private static TerminateResult TerminateP2PQuake()
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

        private static string CalcSha256(string path)
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(fs);
            return BitConverter.ToString(hash).ToLower().Replace("-", "");
        }

        private static string GetAppDirectory()
        {
            return Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        }

        private static string GeneratePath(UpdateEntry entry)
        {
            return GeneratePath(entry.path);
        }

        private static string GeneratePath(string path)
        {
            return Path.Join(GetAppDirectory(), path.Replace('/', Path.DirectorySeparatorChar));
        }
    }
}
