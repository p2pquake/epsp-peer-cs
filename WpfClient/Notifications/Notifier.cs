using Client.App;
using Client.Peer;

using Map.Model;

using Microsoft.Toolkit.Uwp.Notifications;

using Polly;

using Sentry;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WpfClient.EPSPDataView;
using WpfClient.Utils;

namespace WpfClient.Notifications
{
    public class Notifier
    {
        private Configuration configuration;
        private MediatorContext mediatorContext;
        private Dictionary<string, string> userquakeArea;

        public Notifier(Configuration configuration, MediatorContext mediatorContext)
        {
            this.configuration = configuration;
            this.mediatorContext = mediatorContext;
            this.userquakeArea = Resource.epsp_area.Split('\n').Skip(1).Select(e => e.Split(',')).ToDictionary(e => e[0], e => e[4]);

            mediatorContext.OnEarthquake += MediatorContext_OnEarthquake;
            mediatorContext.OnTsunami += MediatorContext_OnTsunami;
            mediatorContext.OnEEW += MediatorContext_OnEEW;
            mediatorContext.OnNewUserquakeEvaluation += MediatorContext_OnNewUserquakeEvaluation;

            ToastNotificationManagerCompat.OnActivated += ToastNotificationManagerCompat_OnActivated;
        }

        private void ToastNotificationManagerCompat_OnActivated(ToastNotificationActivatedEventArgsCompat e)
        {
            var args = ToastArguments.Parse(e.Argument);
            Activator.Activate(
                args["type"],
                args.Contains("receivedAt") ? args["receivedAt"] : null,
                args.Contains("startedAt") ? args["startedAt"] : null
            );
        }

        public void MediatorContext_OnEarthquake(object sender, Client.Peer.EPSPQuakeEventArgs e)
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
            if (!earthquakeNotification.Notice)
            {
                return;
            }

            // 震源情報は震度 3 以上で発表されるため、震度 3 とみなす
            var scale =
                e.InformationType == QuakeInformationType.Destination ?
                30 :
                ScaleConverter.Str2Int(e.Scale);
            if (scale < earthquakeNotification.MinScale)
            {
                return;
            }

            var builder = new ToastContentBuilder();

            // タイトル行
            var type = e.InformationType switch
            {
                QuakeInformationType.ScalePrompt => "震度速報",
                QuakeInformationType.Destination => "震源情報",
                QuakeInformationType.ScaleAndDestination => "震源・震度情報",
                QuakeInformationType.Detail => "地震情報",
                QuakeInformationType.Foreign => "遠地（海外）地震情報",
                _ => "地震情報",
            };

            if (e.InformationType == QuakeInformationType.Foreign || e.InformationType == QuakeInformationType.Destination)
            {
                builder.AddText($"{type} ({e.OccuredTime})");
            } else
            {
                builder.AddText($"{type} ({e.OccuredTime} 震度{e.Scale})");
            }

            if (e.InformationType == QuakeInformationType.ScalePrompt)
            {
                var maxScaleGroup = e.PointList.OrderBy(e => e.ScaleInt).Reverse().GroupBy(e => e.Scale).First();
                builder.AddText($"震度{maxScaleGroup.Key}: {string.Join('、', maxScaleGroup.Select(e => e.Name))}");
            } else
            {
                var tsunamiDescription = e.TsunamiType switch
                {
                    DomesticTsunamiType.None => "津波の心配なし",
                    DomesticTsunamiType.Checking => "津波の有無調査中",
                    DomesticTsunamiType.Effective => "津波予報 発表中",
                    _ => "津波の有無不明",
                };
                builder.AddText($"{e.Destination} (深さ{e.Depth}, M{e.Magnitude}) {tsunamiDescription}");
            }

            builder.AddArgument("type", "quake").AddArgument("receivedAt", e.ReceivedAt.ToString());
            WithRetry(() =>
            {
                builder.Show();
            });
        }

        public void MediatorContext_OnTsunami(object sender, Client.Peer.EPSPTsunamiEventArgs e)
        {
            var tsunamiNotification = configuration.TsunamiNotification;

            if (!tsunamiNotification.Enabled)
            {
                return;
            }
            if (!tsunamiNotification.Notice)
            {
                return;
            }

            if (e.IsCancelled)
            {
                new ToastContentBuilder()
                    .AddText("津波予報 解除")
                    .AddText("津波予報はすべて解除されました。")
                    .AddArgument("type", "tsunami").AddArgument("receivedAt", e.ReceivedAt.ToString())
                    .Show();
                return;
            }

            var maxCategoryGroup = e.RegionList.OrderByDescending(e => e.Category).GroupBy(e => e.Category).First();

            WithRetry(() =>
            {
                new ToastContentBuilder()
                    .AddText("津波予報")
                    .AddText($"{TsunamiCategoryConverter.String(maxCategoryGroup.Key)}: {string.Join('、', maxCategoryGroup.Select(e => $"{(e.IsImmediately ? "＊" : "")}{e.Region}"))} {(e.RegionList.Count != maxCategoryGroup.Count() ? " ほか" : "")}")
                    .AddArgument("type", "tsunami").AddArgument("receivedAt", e.ReceivedAt.ToString())
                    .Show();
            });
        }

        public void MediatorContext_OnEEW(object sender, Client.Peer.EPSPEEWEventArgs e)
        {
            var eewTestNotification = configuration.EEWTestNotification;

            if (!eewTestNotification.Enabled)
            {
                return;
            }
            if (!eewTestNotification.Notice)
            {
                return;
            }

            var hypocenter = $"震源: {EEWConverter.GetHypocenter(e.Hypocenter) ?? "（不明）"}";
            var area = $"強い揺れに警戒: {string.Join(' ', e.Areas.Select(e => EEWConverter.GetArea(e) ?? "（不明）"))}";

            WithRetry(() =>
            {
                new ToastContentBuilder()
                    .AddText("緊急地震速報（警報） 部分配信")
                    .AddText(hypocenter)
                    .AddText(area)
                    .AddArgument("type", "eew").AddArgument("receivedAt", e.ReceivedAt.ToString())
                    .Show();
            });
        }


        public void MediatorContext_OnNewUserquakeEvaluation(object sender, Client.App.Userquake.UserquakeEvaluateEventArgs e)
        {
            var userquakeNotification = configuration.UserquakeNotification;

            if (!userquakeNotification.Enabled)
            {
                return;
            }
            if (!userquakeNotification.Notice)
            {
                return;
            }

            var areas = e.AreaConfidences.Where(e => e.Value.Confidence >= 0 && userquakeArea.ContainsKey(e.Key)).OrderByDescending(e => e.Value.Confidence).Take(3).Select(e => userquakeArea[e.Key]);

            WithRetry(() =>
            {
                new ToastContentBuilder()
                    .AddText("「揺れた！」報告（地震感知情報）")
                    .AddText($"主な地域: {string.Join('、', areas)}")
                    .AddArgument("type", "userquake").AddArgument("startedAt", e.StartedAt.ToString())
                    .Show();
            });
        }

        private void WithRetry(Action a)
        {
            try
            {
                Policy.Handle<Exception>().Retry(3).Execute(a);
            } catch (Exception e)
            {
                SentrySdk.CaptureException(e);
            }
        }
    }
}
