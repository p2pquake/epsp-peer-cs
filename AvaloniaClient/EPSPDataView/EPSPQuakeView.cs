using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;

using Client.Peer;

using Map.Controller;
using Map.Model;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using AvaloniaClient.Utils;
using AvaloniaClient.ViewModels;

namespace AvaloniaClient.EPSPDataView;

public class EPSPQuakeView
{
    public EPSPQuakeEventArgs EventArgs { get; init; } = null!;
    public IFrameModel FrameModel { get; init; } = null!;

    public bool EruptionIconVisible => EventArgs.InformationType == QuakeInformationType.Foreign && (EventArgs.FreeCommentList?.Any(e => e.Contains("大規模な噴火")) ?? false);
    public string EruptionIconSource => IsDarkTheme
        ? "avares://P2PQuake/Resources/Icons/eruption_white.png"
        : "avares://P2PQuake/Resources/Icons/eruption_black.png";

    public bool ForeignIconVisible => EventArgs.InformationType == QuakeInformationType.Foreign && !EruptionIconVisible;
    public bool ScaleVisible => EventArgs.InformationType != QuakeInformationType.Foreign;
    public bool TestLabelVisible => EventArgs?.PointList.Any((point) => point.Name == "テスト震度観測点") ?? false;

    public IBrush ScaleForeground => EventArgs.Scale.CompareTo("5") >= 0 && EventArgs.Scale.CompareTo("7") <= 0
        ? Brushes.Red
        : GetThemeForeground();

    public IBrush TsunamiForeground =>
        EventArgs?.TsunamiType == DomesticTsunamiType.Effective ? Brushes.Red : GetThemeForeground();

    public string TsunamiText => EventArgs?.TsunamiType switch
    {
        DomesticTsunamiType.None => "心配はありません",
        DomesticTsunamiType.Checking => "有無を調査中",
        DomesticTsunamiType.Effective => "津波予報 発表中",
        DomesticTsunamiType.Unknown => "不明",
        null => "不明",
        _ => "不明",
    };

    public bool IsExpanded { get; init; }

    public List<DetailItemView> DetailItemViewList
    {
        get
        {
            var list = new List<DetailItemView>();
            if (EventArgs?.PointList == null) { return list; }

            if (EventArgs?.PointList.Count > 0)
            {
                list.Add(new DetailItemView("各地の震度", TextStyles.Title));
            }

            var regex = StationNameShorter.ShortenPattern;
            var shortenPoints = EventArgs!.PointList.OrderByDescending(e => ConvertScaleIntForSort(e.ScaleInt)).Select(e =>
            {
                var match = regex.Match(e.Name);
                if (match.Success && EventArgs?.InformationType != QuakeInformationType.ScalePrompt)
                {
                    return new QuakeObservationPoint { Prefecture = e.Prefecture, Name = match.Groups[1].Value, Scale = e.Scale };
                }
                return e;
            }).GroupBy(e => e.Name).Select(e => e.First());

            var pointsByPrefs = shortenPoints.GroupBy(e => e.Prefecture);
            foreach (var pointsByPref in pointsByPrefs)
            {
                list.Add(new DetailItemView(pointsByPref.Key, TextStyles.Prefecture));
                var pointsByScales = pointsByPref.GroupBy(e => e.Scale);
                foreach (var pointsByScale in pointsByScales)
                {
                    list.Add(new DetailItemView(string.Join('、', pointsByScale.Select(e => e.Name)), TextStyles.Name, pointsByScale.First().ScaleInt));
                }
            }

            if (EruptionIconVisible)
            {
                list.Add(new DetailItemView("", TextStyles.Eruption));
            }

            if (EventArgs?.FreeCommentList?.Count > 0)
            {
                list.Add(new DetailItemView(string.Join("\n", EventArgs.FreeCommentList), TextStyles.FreeFormComment));
            }

            return list;
        }
    }

    public Bitmap? BitmapImage
    {
        get
        {
            if (EventArgs == null) { return null; }

            var mapDrawer = new MapDrawer()
            {
                MapType = EventArgs.InformationType == QuakeInformationType.Foreign ? MapType.WORLD_1024 : MapType.JAPAN_4096,
                Trim = true,
                Hypocenter = EventArgs.Latitude == "" ? null : new GeoCoordinate(Latitude, Longitude),
                ObservationPoints = GenerateObservationPoints(),
                HideNote = true,
                PreferedAspectRatio = FrameModel.FrameWidth / FrameModel.FrameHeight,
            };
            var png = mapDrawer.DrawAsPng();
            return BitmapHelper.FromStream(png);
        }
    }

    private IList<ObservationPoint> GenerateObservationPoints()
    {
        if (EventArgs.PointList == null) { return new List<ObservationPoint>(); }
        return EventArgs.PointList.Where(e => ConvertScale(e.Scale) > 0).Select(e =>
            new ObservationPoint(e.Prefecture, e.Name, ConvertScale(e.Scale))
        ).ToList();
    }

    private static int ConvertScale(string scale)
    {
        return scale switch
        {
            "1" => 10, "2" => 20, "3" => 30, "4" => 40,
            "5弱" => 45, "5強" => 50, "6弱" => 55, "6強" => 60, "7" => 70,
            "5弱以上（推定）" => 46, _ => -1,
        };
    }

    private static int ConvertScaleIntForSort(int scaleInt) => scaleInt == 46 ? 44 : scaleInt;

    private double Latitude
    {
        get
        {
            var latitude = double.Parse(EventArgs.Latitude.Replace("N", "").Replace("S", ""), NumberFormatInfo.InvariantInfo);
            if (EventArgs.Latitude.StartsWith("S")) latitude *= -1;
            return latitude;
        }
    }

    private double Longitude
    {
        get
        {
            var longitude = double.Parse(EventArgs.Longitude.Replace("W", "").Replace("E", ""), NumberFormatInfo.InvariantInfo);
            if (EventArgs.Longitude.StartsWith("W")) longitude = 360 - longitude;
            return longitude;
        }
    }

    private static bool IsDarkTheme => Application.Current?.ActualThemeVariant == ThemeVariant.Dark;

    private static IBrush GetThemeForeground()
    {
        if (Application.Current != null)
        {
            object? brush = null;
            Application.Current.TryGetResource("TextFillColorPrimaryBrush", Application.Current.ActualThemeVariant, out brush);
            if (brush is IBrush b) return b;
        }
        return Brushes.Black;
    }
}
