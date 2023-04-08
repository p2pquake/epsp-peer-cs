using Client.App;
using Client.Peer;

using NAudio.Wave;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using WpfClient.EPSPDataView;
using WpfClient.Utils;

namespace WpfClient.Notifications
{
    enum SoundType
    {
        P2PQ_Snd0,
        P2PQ_Snd1,
        P2PQ_Snd2,
        P2PQ_Snd3,
        P2PQ_Snd4,
        P2PQ_Snd9,
        P2PQ_Sndt,
        EEW_Beta,
    }

    public class Player
    {
        private Configuration configuration;
        private string lastEarthquakeOccuredTime = "";
        private SoundType lastSoundType = SoundType.P2PQ_Sndt;

        public Player(Configuration configuration, MediatorContext mediatorContext)
        {
            this.configuration = configuration;

            mediatorContext.OnEarthquake += MediatorContext_OnEarthquake;
            mediatorContext.OnTsunami += MediatorContext_OnTsunami;
            mediatorContext.OnEEW += MediatorContext_OnEEW;
            mediatorContext.OnNewUserquakeEvaluation += MediatorContext_OnNewUserquakeEvaluation;
        }

        private static void PlaySoundAsync(SoundType soundType)
        {
            Task.Run(() => PlaySound(soundType));
        }

        private static void PlaySound(SoundType soundType)
        {
            if (WaveOut.DeviceCount <= 0) { return; }
            using (var audioFile = new AudioFileReader(GeneratePath($"Resources/Sounds/{soundType}.mp3")))
            {
                using (var outputDevice = new WaveOutEvent())
                {
                    try
                    {
                        outputDevice.Init(audioFile);
                        outputDevice.Play();
                        while (outputDevice.PlaybackState == PlaybackState.Playing)
                        {
                            Thread.Sleep(1000);
                        }
                    }
                    catch (NAudio.MmException)
                    {
                        // FIXME: あとでログに出力する。
                    }
                }
            }
        }

        private void MediatorContext_OnEarthquake(object sender, EPSPQuakeEventArgs e)
        {
            if (e.InformationType == QuakeInformationType.Unknown)
            {
                return;
            }

            var earthquakeNotification = configuration.EarthquakeNotification;
            if (!earthquakeNotification.Enabled)
            {
                return;
            }
            if (!earthquakeNotification.Sound)
            {
                return;
            }

            var scale = e.InformationType == QuakeInformationType.Destination ? 30 : ScaleConverter.Str2Int(e.Scale);
            if (scale < earthquakeNotification.MinScale)
            {
                return;
            }

            var soundType = calcSoundType(e.InformationType, scale);

            if (e.OccuredTime != lastEarthquakeOccuredTime)
            {
                PlaySoundAsync(soundType);
            } else if (soundType != lastSoundType && lastSoundType != SoundType.P2PQ_Snd0)
            {
                PlaySoundAsync(soundType);
            }

            lastEarthquakeOccuredTime = e.OccuredTime;
            lastSoundType = soundType;
        }

        private SoundType calcSoundType(QuakeInformationType informationType, int scale)
        {
            if (informationType == QuakeInformationType.Destination) { return SoundType.P2PQ_Snd0; }
            if (scale >= 55) { return SoundType.P2PQ_Snd4; }
            if (scale >= 45) { return SoundType.P2PQ_Snd3; }
            if (scale >= 30) { return SoundType.P2PQ_Snd2; }
            return SoundType.P2PQ_Snd1;
        }

        private void MediatorContext_OnTsunami(object sender, EPSPTsunamiEventArgs e)
        {
            var tsunamiNotification = configuration.TsunamiNotification;

            if (!tsunamiNotification.Enabled)
            {
                return;
            }
            if (!tsunamiNotification.Sound)
            {
                return;
            }

            PlaySoundAsync(SoundType.P2PQ_Sndt);
        }

        private void MediatorContext_OnEEW(object sender, EPSPEEWEventArgs e)
        {
            var eewTestNotification = configuration.EEWTestNotification;

            if (!eewTestNotification.Enabled)
            {
                return;
            }

            if (eewTestNotification.Sound)
            {
                PlaySoundAsync(SoundType.EEW_Beta);
            }

            if (eewTestNotification.VoiceGuidance)
            {
                SoundPlayerEEW.PlayEEWAsync(e);
            }
        }

        private void MediatorContext_OnNewUserquakeEvaluation(object sender, Client.App.Userquake.UserquakeEvaluateEventArgs e)
        {
            var userquakeNotification = configuration.UserquakeNotification;

            if (!userquakeNotification.Enabled)
            {
                return;
            }
            if (!userquakeNotification.Sound)
            {
                return;
            }

            PlaySoundAsync(SoundType.P2PQ_Snd9);
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
