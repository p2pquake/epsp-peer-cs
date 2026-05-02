using Client.App;
using Client.Peer;

using AvaloniaClient.Services;
using AvaloniaClient.Utils;
using AvaloniaClient.ViewModels;

using Map.Model;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvaloniaClient.Notifications;

public class Notifier
{
    private Configuration configuration;
    private MediatorContext mediatorContext;
    private RootViewModel viewModel;
    private Dictionary<string, string> userquakeArea;

    public Notifier(Configuration configuration, MediatorContext mediatorContext, RootViewModel viewModel)
    {
        this.configuration = configuration;
        this.mediatorContext = mediatorContext;
        this.viewModel = viewModel;
        this.userquakeArea = AreaDataProvider.AreaDictionary;

        mediatorContext.OnEarthquake += (s, e) => { Task.Run(() => MediatorContext_OnEarthquake(s, e)); };
        mediatorContext.OnTsunami += (s, e) => { Task.Run(() => MediatorContext_OnTsunami(s, e)); };
        mediatorContext.OnEEW += (s, e) => { Task.Run(() => MediatorContext_OnEEW(s, e)); };
        mediatorContext.OnNewUserquakeEvaluation += (s, e) => { Task.Run(() => MediatorContext_OnNewUserquakeEvaluation(s, e)); };
    }

    private void ShowNotification(string title, string message, string type, string? receivedAt = null, string? startedAt = null)
    {
        OsNotifier.Show(title, message);
    }

    private void MediatorContext_OnEarthquake(object? sender, EPSPQuakeEventArgs e)
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
        var foreign = e.InformationType == QuakeInformationType.Foreign && earthquakeNotification.Foreign;
        if (scale < earthquakeNotification.MinScale && !foreign)
        {
            return;
        }

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

        string title;
        if (e.InformationType == QuakeInformationType.Foreign || e.InformationType == QuakeInformationType.Destination)
        {
            title = $"{type} ({e.OccuredTime})";
        }
        else
        {
            title = $"{type} ({e.OccuredTime} 震度{e.Scale})";
        }

        string body;
        if (e.InformationType == QuakeInformationType.ScalePrompt)
        {
            var maxScaleGroup = e.PointList.OrderBy(p => p.ScaleInt).Reverse().GroupBy(p => p.Scale).First();
            body = $"震度{maxScaleGroup.Key}: {string.Join('、', maxScaleGroup.Select(p => p.Name))}";
        }
        else
        {
            var tsunamiDescription = e.TsunamiType switch
            {
                DomesticTsunamiType.None => "津波の心配なし",
                DomesticTsunamiType.Checking => "津波の有無調査中",
                DomesticTsunamiType.Effective => "津波予報 発表中",
                _ => "津波の有無不明",
            };
            body = $"{e.Destination} (深さ{e.Depth}, M{e.Magnitude}) {tsunamiDescription}";
        }

        ShowNotification(title, body, "quake", e.ReceivedAt.ToString());
    }

    private void MediatorContext_OnTsunami(object? sender, EPSPTsunamiEventArgs e)
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
            ShowNotification("津波予報 解除", "津波予報はすべて解除されました。", "tsunami", e.ReceivedAt.ToString());
            return;
        }

        var maxCategoryGroup = e.RegionList.OrderByDescending(r => r.Category).GroupBy(r => r.Category).First();
        var body = $"{TsunamiCategoryConverter.String(maxCategoryGroup.Key)}: {string.Join('、', maxCategoryGroup.Select(r => $"{(r.IsImmediately ? "＊" : "")}{r.Region}"))} {(e.RegionList.Count != maxCategoryGroup.Count() ? " ほか" : "")}";

        ShowNotification("津波予報", body, "tsunami", e.ReceivedAt.ToString());
    }

    private void MediatorContext_OnEEW(object? sender, EPSPEEWEventArgs e)
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
        var area = $"強い揺れに警戒: {string.Join(' ', e.Areas.Select(a => EEWConverter.GetArea(a) ?? "（不明）"))}";

        string title;
        string body;
        if (e.IsCancelled)
        {
            title = "緊急地震速報（警報） 取消";
            body = "緊急地震速報（警報）は取り消されました";
        }
        else
        {
            title = "緊急地震速報（警報）" + (e.IsFollowUp ? " 続報" : "");
            body = $"{hypocenter}\n{area}";
        }

        ShowNotification(title, body, "eew", e.ReceivedAt.ToString());
    }

    private void MediatorContext_OnNewUserquakeEvaluation(object? sender, Client.App.Userquake.UserquakeEvaluateEventArgs e)
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

        var areas = e.AreaConfidences.Where(a => a.Value.Confidence >= 0 && userquakeArea.ContainsKey(a.Key)).OrderByDescending(a => a.Value.Confidence).Take(3).Select(a => userquakeArea[a.Key]);

        ShowNotification("「揺れた！」報告（地震感知情報）", $"主な地域: {string.Join('、', areas)}", "userquake", null, e.StartedAt.ToString());
    }
}
