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
using System.Windows;

namespace Updater
{
    public class UpdateClient
    {
        public enum UpdateResult
        {
            Failure,
            Success,
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
            foreach (var entry in entries)
            {
                // 自分自身が更新対象の場合、 P2PQuakeAutoUpdater2.exe として退避する
                // （あとで P2PQuake (WpfClient) によって差し替えられる）
                if (entry.path == "P2PQuakeAutoUpdater.exe")
                {
                    entry.path = "P2PQuakeAutoUpdater2.exe";
                }

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
                // ... つもりだったが、差し替え時使用中だと .TMP ファイルが生成されて残る挙動がみられたため止めた。
                //if (File.Exists(path))
                //{
                //    File.Replace($"{path}.tmp", path, null);
                //} else
                //{
                File.Move($"{path}.tmp", path, true);
                //}
            }

            return UpdateResult.Success;
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
            return Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
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
