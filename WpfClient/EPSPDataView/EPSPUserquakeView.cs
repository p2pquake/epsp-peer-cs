using Client.App.Userquake;

using Map.Controller;
using Map.Model;

using ModernWpf;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace WpfClient.EPSPDataView
{
    public class EPSPUserquakeView
    {
        public UserquakeEvaluateEventArgs EventArgs { get; init; }

        public string Source => ThemeManager.Current.ActualApplicationTheme switch
        {
            ApplicationTheme.Light => "/Resources/Icons/userquake_black.png",
            ApplicationTheme.Dark => "/Resources/Icons/userquake_white.png",
            _ => throw new NotImplementedException(),
        };

        public string Time => EventArgs.StartedAt.ToString("dd日HH時mm分");

        public string Caption => "地震感知情報";

        // FIXME: 受信中か判定するために MediatorContext (Client) からプロトコル日時を取る必要がある
        public Visibility ReceivingVisibility =>
            Visibility.Hidden;

        public string DetailTime => $"{EventArgs?.StartedAt.ToString("M月dd日HH時mm分ss秒")}～{EventArgs?.UpdatedAt.ToString("HH時mm分ss秒")}";

        public BitmapImage BitmapImage
        {
            get
            {
                if (EventArgs == null) { return null; }

                var mapDrawer = new MapDrawer()
                {
                    MapType = Map.Model.MapType.JAPAN_4096,
                    Trim = true,
                    UserquakePoints = GenerateUserquakePoints(),
                    HideNote = true,
                };
                var png = mapDrawer.DrawAsPng();

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = png;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }

        private IList<UserquakePoint> GenerateUserquakePoints()
        {
            return EventArgs.AreaConfidences.Select(e => new UserquakePoint(e.Value.AreaCode, e.Value.Confidence)).ToList();
        }
    }
}
