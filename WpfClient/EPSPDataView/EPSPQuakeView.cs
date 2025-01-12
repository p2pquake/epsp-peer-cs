using Client.Peer;

using Map.Controller;
using Map.Model;

using ModernWpf;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WpfClient.EPSPDataView
{
    public class EPSPQuakeView
    {
        public EPSPQuakeEventArgs EventArgs { get; init; }
        public IFrameModel FrameModel { get; init; }

        public Visibility EruptionIconVisiility => EventArgs.InformationType == QuakeInformationType.Foreign && (EventArgs.FreeCommentList?.Any(e => e.Contains("大規模な噴火")) ?? false) ? Visibility.Visible : Visibility.Collapsed;
        public string EruptionIconSource => ThemeManager.Current.ActualApplicationTheme switch
                {
                    ApplicationTheme.Light => "/Resources/Icons/eruption_black.png",
                    ApplicationTheme.Dark => "/Resources/Icons/eruption_white.png",
                    _ => throw new NotImplementedException(),
                };
        public Visibility ForeignIconVisibility => EventArgs.InformationType == QuakeInformationType.Foreign && EruptionIconVisiility != Visibility.Visible ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ScaleVisibility => EventArgs.InformationType == QuakeInformationType.Foreign ? Visibility.Collapsed : Visibility.Visible;
        public Visibility TestLabelVisibility => (EventArgs?.PointList.Any((point) => point.Name == "テスト震度観測点") ?? false) ? Visibility.Visible : Visibility.Collapsed;

        public SolidColorBrush ScaleForeground => EventArgs.Scale.CompareTo("5") >= 0 && EventArgs.Scale.CompareTo("7") <= 0
                    ? (SolidColorBrush)App.Current.MainWindow.FindResource("SystemControlErrorTextForegroundBrush")
                    : (SolidColorBrush)App.Current.MainWindow.FindResource("SystemControlPageTextBaseHighBrush");

        public SolidColorBrush TsunamiForeground =>
            EventArgs?.TsunamiType == DomesticTsunamiType.Effective ? (SolidColorBrush)App.Current.MainWindow.FindResource("SystemControlErrorTextForegroundBrush") :
            (SolidColorBrush)App.Current.MainWindow.FindResource("SystemControlPageTextBaseHighBrush");

        public string TsunamiText => EventArgs?.TsunamiType switch
        {
            DomesticTsunamiType.None => "心配はありません",
            DomesticTsunamiType.Checking => "有無を調査中",
            DomesticTsunamiType.Effective => "津波予報 発表中",
            DomesticTsunamiType.Unknown => "不明",
            null => "不明",
            _ => throw new NotImplementedException(),
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

                // 市区町村名に省略する
                var regex = StationNameShorter.ShortenPattern;
                var shortenPoints = EventArgs.PointList.OrderByDescending(e => ConvertScaleIntForSort(e.ScaleInt)).Select(e =>
                {
                    var match = regex.Match(e.Name);
                    if (match.Success && EventArgs?.InformationType != QuakeInformationType.ScalePrompt)
                    {
                        return new QuakeObservationPoint { Prefecture = e.Prefecture, Name = match.Groups[1].Value, Scale = e.Scale };
                    }
                    return e;
                }).GroupBy(e => e.Name).Select(e => e.First());

                // XXX: もっときれいに書きたい～
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

                // 自由付加文
                if (EruptionIconVisiility == Visibility.Visible)
                {
                    list.Add(new DetailItemView("", TextStyles.Eruption));
                }

                if (EventArgs?.FreeCommentList.Count > 0)
                {
                    list.Add(new DetailItemView(string.Join("\n", EventArgs?.FreeCommentList), TextStyles.FreeFormComment));
                }

                return list;
            }
        }

        public BitmapImage BitmapImage
        {
            get
            {
                if (EventArgs == null) { return null; }

                var mapDrawer = new MapDrawer()
                {
                    MapType = EventArgs.InformationType == QuakeInformationType.Foreign ? Map.Model.MapType.WORLD_1024 : Map.Model.MapType.JAPAN_4096,
                    Trim = true,
                    Hypocenter = EventArgs.Latitude == "" ? null : new Map.Model.GeoCoordinate(Latitude, Longitude),
                    ObservationPoints = GenerateObservationPoints(),
                    HideNote = true,
                    PreferedAspectRatio = FrameModel.FrameWidth / FrameModel.FrameHeight,
                };
                var png = mapDrawer.DrawAsPng();

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = png;
                bitmapImage.EndInit();
                return bitmapImage;
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
                "1" => 10,
                "2" => 20,
                "3" => 30,
                "4" => 40,
                "5弱" => 45,
                "5強" => 50,
                "6弱" => 55,
                "6強" => 60,
                "7" => 70,
                "5弱以上（推定）" => 46,
                _ => -1,
            };
        }

        /// <summary>震度 5 弱 → 震度 5 弱以上（推定） と並ぶよう修正する。 API 仕様を直したいくらいの設計ミス感。</summary>
        private static int ConvertScaleIntForSort(int scaleInt)
        {
            return scaleInt == 46 ? 44 : scaleInt;
        }

        private double Latitude
        {
            get
            {
                var latitude = double.Parse(EventArgs.Latitude.Replace("N", "").Replace("S", ""), NumberFormatInfo.InvariantInfo);
                if (EventArgs.Latitude.StartsWith("S"))
                {
                    latitude *= -1;
                }
                return latitude;
            }
        }

        private double Longitude
        {
            get
            {
                var longitude = double.Parse(EventArgs.Longitude.Replace("W", "").Replace("E", ""), NumberFormatInfo.InvariantInfo);
                if (EventArgs.Longitude.StartsWith("W"))
                {
                    // TODO: たぶんあってるけど自信がない
                    longitude = 360 - longitude;
                }
                return longitude;
            }
        }
    }
}
