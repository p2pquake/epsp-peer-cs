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

namespace AutoUpdater
{
    public class UpdateClient
    {
        enum TerminateResult
        {
            NotRunning,
            Terminated,
            Running,
        }

        private static string UpdateUri = "https://www.p2pquake.net/update";
        private static HttpClient Client = new HttpClient() { Timeout = TimeSpan.FromSeconds(30) };

        private static TerminateResult TerminateP2PQuake()
        {
            // プロセスは起動してる？
            var processes = Process.GetProcessesByName("P2PQuake");
            if (processes.Length <= 0)
            {
                return TerminateResult.NotRunning;
            }

            // パイプを開き、終了してもらう
            using var pipe = new NamedPipeClientStream(".", "p2pquake-ipc", PipeDirection.Out, PipeOptions.CurrentUserOnly);
            try
            {
                pipe.Connect(1000);
                using var stream = new StreamWriter(pipe);
                stream.AutoFlush = true;
                stream.WriteLine(JsonSerializer.Serialize(new Dictionary<string, string>() { { "method", "exit" } }));
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

            return TerminateResult.Running;
        }

        public static async Task UpdateAsync(UpdateEntry[] entries)
        {
            var appDirectory = GetAppDirectory();

            TerminateP2PQuake();

            foreach (var entry in entries)
            {
                // FIXME: 例外処理
                var response = await Client.GetStreamAsync($"{UpdateUri}/{entry.path}");

                // ディレクトリが必要なら作成する
                var path = GeneratePath(entry);
                var directory = Path.GetDirectoryName(path);

                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // ファイルを書き込む
                using var fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write);
                response.CopyTo(fs);
                fs.Close();

                // 検証しとく？
                if (CalcSha256(path) != entry.sha256Digest)
                {
                    // FIXME: 例外を出したところで表示に反映されるわけではないので現時点で意味がない。
                    throw new Exception($"ファイル {entry.path} のハッシュ値が異なります。");
                }
            }
        }

        public static async Task<UpdateEntry[]> CheckUpdateAsync()
        {
            // FIXME: 例外処理

            // アップデート情報を取得
            var response = await Client.GetStringAsync($"{UpdateUri}/update.json");
            var entries = JsonSerializer.Deserialize<UpdateEntry[]>(response);

            // 更新差分だけ抽出
            var diffEntries = entries.Where(entry =>
            {
                var path = GeneratePath(entry);
                // ファイルがない
                if (entry.required && !File.Exists(path))
                {
                    return true;
                }

                // ファイルはあるが、ハッシュ値が異なる
                if (!entry.allowDigestMismatch && File.Exists(path) && CalcSha256(path) != entry.sha256Digest)
                {
                    return true;
                }

                return false;
            });

            // 差分を返却
            return diffEntries.ToArray();
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
            return Path.Join(GetAppDirectory(), entry.path.Replace('/', Path.DirectorySeparatorChar));
        }
    }
}
