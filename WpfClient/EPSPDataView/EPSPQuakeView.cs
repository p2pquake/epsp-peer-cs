using Client.Peer;

using Map.Controller;
using Map.Model;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WpfClient.EPSPDataView
{
    public class EPSPQuakeView
    {
        public EPSPQuakeEventArgs EventArgs { get; init; }
        public IFrameModel FrameModel { get; init; }

        public Visibility ForeignIconVisibility => EventArgs.InformationType == QuakeInformationType.Foreign ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ScaleVisibility => EventArgs.InformationType == QuakeInformationType.Foreign ? Visibility.Collapsed : Visibility.Visible;

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

        public List<DetailItemView> DetailItemViewList
        {
            get
            {
                var list = new List<DetailItemView>();

                if (EventArgs?.PointList == null) { return list; }

                list.Add(new DetailItemView("各地の震度", TextStyles.Title));

                // XXX: もっときれいに書きたい～
                var pointsByPrefs = EventArgs.PointList.OrderBy(e => e.Scale).Reverse().GroupBy(e => e.Prefecture);
                foreach (var pointsByPref in pointsByPrefs)
                {
                    list.Add(new DetailItemView(pointsByPref.Key, TextStyles.Prefecture));
                    var pointsByScales = pointsByPref.GroupBy(e => e.Scale);
                    foreach (var pointsByScale in pointsByScales)
                    {
                        list.Add(new DetailItemView($"震度{pointsByScale.Key}", TextStyles.Scale));
                        list.Add(new DetailItemView(string.Join('、', pointsByScale.Select(e => e.Name)), TextStyles.Name));
                    }
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

            return EventArgs.PointList.Select(e =>
                new ObservationPoint(e.Prefecture, e.Name, ConvertScale(e.Scale))
            ).ToList();
        }

        // FIXME: あとでなおす。
        private int ConvertScale(string scale)
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
            };
        }

        private double Latitude
        {
            get
            {
                var latitude = double.Parse(EventArgs.Latitude.Replace("N", "").Replace("S", ""));
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
                var longitude = double.Parse(EventArgs.Longitude.Replace("W", "").Replace("E", ""));
                if (EventArgs.Longitude.StartsWith("W"))
                {
                    // FIXME: わすれたのでいったん適当に
                    longitude = 360 - longitude;
                }
                return longitude;
            }
        }
    }
}
