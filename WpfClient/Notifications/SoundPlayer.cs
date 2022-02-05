using Client.App;
using Client.Peer;

using NAudio.Wave;

using System;
using System.Collections.Generic;
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

        public Player(Configuration configuration, MediatorContext mediatorContext)
        {
            this.configuration = configuration;

            mediatorContext.OnEarthquake += MediatorContext_OnEarthquake;
            mediatorContext.OnTsunami += MediatorContext_OnTsunami;
            mediatorContext.OnEEWTest += MediatorContext_OnEEWTest;
            mediatorContext.OnNewUserquakeEvaluation += MediatorContext_OnNewUserquakeEvaluation;
        }

        private static async void PlaySound(SoundType soundType)
        {
            if (WaveOut.DeviceCount <= 0) { return; }
            using(var audioFile = new AudioFileReader($"Resources/Sounds/{soundType}.mp3")) {
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
                    } catch (NAudio.MmException)
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

            if (e.OccuredTime == lastEarthquakeOccuredTime)
            {
                return;
            }
            lastEarthquakeOccuredTime = e.OccuredTime;

            var scale = e.InformationType == QuakeInformationType.Destination ? 30 : ScaleConverter.Str2Int(e.Scale);
            if (scale < earthquakeNotification.MinScale)
            {
                return;
            }

            if (e.InformationType == QuakeInformationType.Destination)
            {
                PlaySound(SoundType.P2PQ_Snd0);
                return;
            }

            if (scale >= 55)
            {
                PlaySound(SoundType.P2PQ_Snd4);
            } else if (scale >= 45)
            {
                PlaySound(SoundType.P2PQ_Snd3);
            } else if (scale >= 30)
            {
                PlaySound(SoundType.P2PQ_Snd2);
            } else
            {
                PlaySound(SoundType.P2PQ_Snd1);
            }
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

            PlaySound(SoundType.P2PQ_Sndt);
        }

        private void MediatorContext_OnEEWTest(object sender, EPSPEEWTestEventArgs e)
        {
            var eewTestNotification = configuration.EEWTestNotification;

            if (!eewTestNotification.Enabled)
            {
                return;
            }
            if (!eewTestNotification.Sound)
            {
                return;
            }

            PlaySound(SoundType.EEW_Beta);
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

            PlaySound(SoundType.P2PQ_Snd9);
        }
    }
}
