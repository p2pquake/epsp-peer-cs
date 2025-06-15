using Avalonia.Media.Imaging;

using Client.Peer;

using Map.Controller;
using Map.Model;

using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace AvaloniaUIClient.ViewModels.Information
{
    public class EarthquakeViewModel : ViewModelBase
    {
        private EPSPQuakeEventArgs eventArgs;
        private InformationViewModel vm;

        public EarthquakeViewModel(EPSPQuakeEventArgs eventArgs, InformationViewModel vm)
        {
            this.eventArgs = eventArgs;
            this.vm = vm;
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
                    DomesticTsunamiType.Effective => "⚠発表中",
                    DomesticTsunamiType.Checking => "有無を調査中",
                    DomesticTsunamiType.Unknown => "有無は不明",
                    _ => "不明（情報なし）",
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
    }
}
