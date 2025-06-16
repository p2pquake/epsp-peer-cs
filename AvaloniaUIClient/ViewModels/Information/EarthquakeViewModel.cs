using Avalonia.Media;
using Avalonia.Media.Imaging;
using AvaloniaUIClient.Models;

using Client.Peer;

using Map.Controller;
using Map.Model;

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AvaloniaUIClient.ViewModels.Information
{
    public partial class EarthquakeViewModel : ViewModelBase
    {
        private EPSPQuakeEventArgs eventArgs;
        private InformationViewModel vm;

        [ObservableProperty]
        private bool isDetailPaneOpen = false;

        [ObservableProperty]
        private ObservableCollection<DetailItemView> detailItemViewList = new();

        public EarthquakeViewModel(EPSPQuakeEventArgs eventArgs, InformationViewModel vm)
        {
            this.eventArgs = eventArgs;
            this.vm = vm;
            InitializeDetailItemViewList();
        }

        public string DetailPaneToggleIcon => IsDetailPaneOpen ? "▶" : "◀";

        [RelayCommand]
        public void ToggleDetailPane()
        {
            IsDetailPaneOpen = !IsDetailPaneOpen;
            OnPropertyChanged(nameof(DetailPaneToggleIcon));
        }

        public string OccuredAt { get { return eventArgs.OccuredTime; } }
        public string Destination
        {
            get
            {
                if (eventArgs.InformationType == QuakeInformationType.ScalePrompt)
                {
                    return "調査中";
                }
                return eventArgs.Destination;
            }
        }
        public string Depth
        {
            get
            {
                if (eventArgs.InformationType == QuakeInformationType.ScalePrompt)
                {
                    return "-";
                }
                if (eventArgs.Depth == "-1" || eventArgs.Depth == "")
                {
                    return "-";
                }
                if (eventArgs.Depth == "0")
                {
                    return "ごく浅い";
                }
                return eventArgs.Depth;
            }
        }
        public string Magnitude
        {
            get
            {
                if (eventArgs.InformationType == QuakeInformationType.ScalePrompt)
                {
                    return "-";
                }
                if (eventArgs.Magnitude == "-1" || eventArgs.Magnitude == "")
                {
                    return "-";
                }
                return $"M{double.Parse(eventArgs.Magnitude):F1}";
            }
        }
        public string TsunamiLabel
        {
            get => eventArgs.InformationType == QuakeInformationType.Foreign ? "日本への津波" : "津波";
        }
        public string DomesticTsunami
        {
            get
            {
                if (eventArgs.InformationType == QuakeInformationType.ScalePrompt)
                {
                    return "有無を調査中";
                }

                return eventArgs.TsunamiType switch
                {
                    DomesticTsunamiType.None => "心配はありません",
                    DomesticTsunamiType.Effective => "津波予報 発表中",
                    DomesticTsunamiType.Checking => "有無を調査中",
                    DomesticTsunamiType.Unknown => "不明",
                    _ => "不明",
                };
            }
        }

        public IBrush TsunamiForeground
        {
            get
            {
                return eventArgs.TsunamiType switch
                {
                    DomesticTsunamiType.Effective => new SolidColorBrush(Color.FromRgb(196, 43, 28)), // SystemControlErrorTextForegroundBrush相当
                    _ => new SolidColorBrush(Color.FromRgb(0, 0, 0)), // SystemControlPageTextBaseHighBrush相当
                };
            }
        }

        public Bitmap BitmapImage
        {
            get
            {
                if (eventArgs == null) { return null; }

                var mapDrawer = new MapDrawer()
                {
                    MapType = eventArgs.InformationType == QuakeInformationType.Foreign ? Map.Model.MapType.WORLD_1024 : Map.Model.MapType.JAPAN_4096,
                    Trim = true,
                    Hypocenter = eventArgs.Latitude == "" ? null : new Map.Model.GeoCoordinate(Latitude, Longitude),
                    ObservationPoints = GenerateObservationPoints(),
                    HideNote = true,
                    PreferedAspectRatio = vm.BodyWidth > 0 ? (vm.BodyWidth - 48) / (vm.BodyHeight - 32) : 0,
                };
                var png = mapDrawer.DrawAsPng();
                var bitmap = new Bitmap(png);
                return bitmap;
            }
        }

        // TODO: 以下、 WpfClient と同じ実装。要共通化
        private IList<ObservationPoint> GenerateObservationPoints()
        {
            if (eventArgs.PointList == null) { return new List<ObservationPoint>(); }

            return eventArgs.PointList.Where(e => ConvertScale(e.Scale) > 0).Select(e =>
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
                var latitude = double.Parse(eventArgs.Latitude.Replace("N", "").Replace("S", ""), NumberFormatInfo.InvariantInfo);
                if (eventArgs.Latitude.StartsWith("S"))
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
                var longitude = double.Parse(eventArgs.Longitude.Replace("W", "").Replace("E", ""), NumberFormatInfo.InvariantInfo);
                if (eventArgs.Longitude.StartsWith("W"))
                {
                    // TODO: たぶんあってるけど自信がない
                    longitude = 360 - longitude;
                }
                return longitude;
            }
        }

        private void InitializeDetailItemViewList()
        {
            var list = new List<DetailItemView>();

            // ヘッダー追加
            list.Add(new DetailItemView("各地の震度", TextStyles.Title));

            if (eventArgs.PointList == null || !eventArgs.PointList.Any())
            {
                DetailItemViewList = new ObservableCollection<DetailItemView>(list);
                return;
            }

            // 観測点名の短縮処理（WPFClientと同様）
            var shortenPoints = eventArgs.PointList
                .Where(p => !string.IsNullOrEmpty(p.Scale))
                .OrderByDescending(e => ConvertScaleIntForSort(ConvertScale(e.Scale)))
                .Select(e => new
                {
                    Name = ShortenLocationName(e.Name),
                    Prefecture = e.Prefecture,
                    Scale = e.Scale,
                    ScaleInt = ConvertScale(e.Scale)
                })
                .GroupBy(e => e.Name)
                .Select(g => g.First())
                .ToList();

            // 都道府県別にグループ化
            var pointsByPrefs = shortenPoints.GroupBy(e => e.Prefecture);
            foreach (var pointsByPref in pointsByPrefs)
            {
                list.Add(new DetailItemView(pointsByPref.Key, TextStyles.Prefecture));
                
                // 震度別にグループ化
                var pointsByScales = pointsByPref.GroupBy(e => e.Scale);
                foreach (var pointsByScale in pointsByScales)
                {
                    var names = string.Join('、', pointsByScale.Select(e => e.Name));
                    list.Add(new DetailItemView(names, TextStyles.Name, pointsByScale.First().ScaleInt));
                }
            }

            DetailItemViewList = new ObservableCollection<DetailItemView>(list);
        }

        private static string ShortenLocationName(string name)
        {
            // WPFClientと同様の正規表現による地名短縮
            var patterns = new[]
            {
                @"^(.+?郡)(.+?[町村])(.*)$",
                @"^(.+?市)(.+?区)(.*)$", 
                @"^(.+?[市町村])(.*)$"
            };

            foreach (var pattern in patterns)
            {
                var match = System.Text.RegularExpressions.Regex.Match(name, pattern);
                if (match.Success)
                {
                    if (match.Groups.Count >= 3 && !string.IsNullOrEmpty(match.Groups[2].Value))
                    {
                        return match.Groups[2].Value;
                    }
                    else if (match.Groups.Count >= 2)
                    {
                        return match.Groups[1].Value;
                    }
                }
            }
            return name;
        }
    }
}
