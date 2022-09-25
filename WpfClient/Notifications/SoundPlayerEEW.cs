using Client.Peer;

using NAudio.Wave;

using Sentry;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WpfClient.Notifications
{
    internal class SoundPlayerEEW
    {
        // 重複再生しないための手抜き制御
        private static string playingUUID = "";


        public static void PlayEEWAsync(EPSPEEWEventArgs eew)
        {
            playingUUID = Guid.NewGuid().ToString();

            Task.Run(() =>
            {
                PlayEEW(eew, playingUUID);
            });
        }

        private static void PlayEEW(EPSPEEWEventArgs eew, string uuid)
        {
            var playIds = new List<string>();
            if (eew.Areas == null || eew.Areas.Length == 0)
            {
                playIds.Add("eew");
                playIds.Add("guidance");
            } else
            {
                playIds.Add("eew");
                playIds.Add("announce_areas");
                foreach (var area in eew.Areas)
                {
                    playIds.Add($"_{area}");
                }
                playIds.AddRange(playIds.ToArray());
                playIds.Add("guidance");
            }

            foreach (var playId in playIds)
            {
                try
                {
                    if (uuid != playingUUID) { return; }
                    byte[] bytes = (byte[])EEWVoice.ResourceManager.GetObject(playId);
                    PlaySound(bytes);
                } catch (Exception e)
                {
                    // 動作継続を優先
                    SentrySdk.CaptureException(e);
                }
            }
        }

        private static void PlaySound(byte[] bytes)
        {
            if (WaveOut.DeviceCount <= 0) { return; }

            using var stream = new MemoryStream(bytes);
            using var audioFile = new Mp3FileReader(stream);
            using var outputDevice = new WaveOutEvent();

            try
            {
                outputDevice.Init(audioFile);
                outputDevice.Play();
                while (outputDevice.PlaybackState == PlaybackState.Playing)
                {
                    Thread.Sleep(100);
                }
            }
            catch (NAudio.MmException)
            {
                // FIXME: あとでログに出力する。
            }
        }
    }
}
