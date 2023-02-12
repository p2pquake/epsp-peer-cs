using Client.App.Userquake;

using Map.Controller;
using Map.Model;

using ModernWpf;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WpfClient.EPSPDataView
{
    public class EPSPUserquakeView : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /*
         * 地震感知情報の地図描画
         * 
         * - 操作性改善のため、地図生成を非同期で行います。
         * - 負荷軽減のため、シングルスレッドで生成し、追いつかない分は生成を省略します。
         */
        private ConcurrentQueue<UserquakeEvaluateEventArgs> drawingQueue = new ConcurrentQueue<UserquakeEvaluateEventArgs>();

        // 地震感知情報の更新時は EventArgs を上書きする
        private UserquakeEvaluateEventArgs eventArgs;
        public UserquakeEvaluateEventArgs EventArgs
        {
            get => eventArgs;
            set
            {
                eventArgs = value;
                drawingQueue.Enqueue(value);
                OnPropertyChanged();
                OnPropertyChanged("DetailTime");
                OnPropertyChanged("Count");
                OnPropertyChanged("Rate");
                OnPropertyChanged("UserquakeDetails");
            }
        }

        public IFrameModel FrameModel { get; init; }

        public Visibility TestLabelVisibility => (EventArgs?.AreaConfidences.Any((area) => area.Key == "-1") ?? false) ? Visibility.Visible : Visibility.Collapsed;

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
            EventArgs != null && DateTime.Now.Subtract(EventArgs.UpdatedAt).TotalSeconds < 40 ? Visibility.Visible : Visibility.Hidden;

        public string DetailTime => $"{EventArgs?.StartedAt.ToString("M月dd日HH時mm分ss秒")}～{EventArgs?.UpdatedAt.ToString("HH時mm分ss秒")}";

        public string Count => EventArgs == null ? "不明" : $"{EventArgs.Count} 件";

        public string Rate
        {
            get
            {
                if (EventArgs == null) { return "-"; }

                var diff = EventArgs.UpdatedAt.Subtract(EventArgs.StartedAt).TotalSeconds;
                if (diff <= 0)
                {
                    return "計測中";
                }

                return $"{EventArgs.Count / diff:N2}/sec";
            }
        }

        public record UserquakeDetail(double Confidence, string Label, Brush brush);

        public List<UserquakeDetail> UserquakeDetails
        {
            get
            {
                if (EventArgs == null) { return new List<UserquakeDetail>(); }

                return GenerateUserquakePoints(EventArgs).Where(e => areas.ContainsKey(e.Areacode)).OrderByDescending(e => e.Confidence).Select(e => new UserquakeDetail(e.Confidence * 80, $"{areas[e.Areacode]} ({EventArgs.AreaConfidences[e.Areacode].Count} 件)", new SolidColorBrush(ConvConfidenceColor(e.Confidence)))).ToList();
            }
        }

        private byte[] pngImage = Resource.loading;
        private byte[] PngImage
        {
            get => PngImage;
            set
            {
                lock (pngImage)
                {
                    pngImage = value;
                }
                OnPropertyChanged("BitmapImage");
            }
        }

        public BitmapImage BitmapImage
        {
            get
            {
                lock (pngImage)
                {
                    if (pngImage == null) { return null; }
                    if (eventArgs != null && ReceivingVisibility == Visibility.Hidden)
                    {
                        var mapDrawer = new MapDrawer()
                        {
                            MapType = Map.Model.MapType.JAPAN_2048,
                            Trim = true,
                            UserquakePoints = GenerateUserquakePoints(EventArgs),
                            HideNote = true,
                            PreferedAspectRatio = (FrameModel.FrameWidth - 240) / (FrameModel.FrameHeight - 40),
                        };
                        var png = mapDrawer.DrawAsPng();
                        using (var ms = new MemoryStream())
                        {
                            png.CopyTo(ms);
                            pngImage = ms.ToArray();
                        }
                    }

                    var bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = new MemoryStream(pngImage);
                    bitmapImage.EndInit();
                    bitmapImage.Freeze();
                    GC.Collect();
                    return bitmapImage;
                }
            }
        }

        public EPSPUserquakeView()
        {
        }

        public EPSPUserquakeView(UserquakeEvaluateEventArgs eventArgs, IFrameModel frameModel)
        {
            this.EventArgs = eventArgs;
            this.FrameModel = frameModel;

            // 画像生成スレッド
            _ = Task.Run(() =>
            {
                var firstRendering = true;
                while (firstRendering || ReceivingVisibility == Visibility.Visible)
                {
                    firstRendering = false;
                    Thread.Sleep(250);

                    UserquakeEvaluateEventArgs eventArgs = null;
                    while (!drawingQueue.IsEmpty) { drawingQueue.TryDequeue(out eventArgs);  }

                    if (eventArgs != null)
                    {
                        var mapDrawer = new MapDrawer()
                        {
                            MapType = Map.Model.MapType.JAPAN_2048,
                            Trim = true,
                            UserquakePoints = GenerateUserquakePoints(eventArgs),
                            HideNote = true,
                            PreferedAspectRatio = (FrameModel.FrameWidth - 240) / (FrameModel.FrameHeight - 40),

                        };
                        var png = mapDrawer.DrawAsPng();
                        using (var ms = new MemoryStream())
                        {
                            png.CopyTo(ms);
                            PngImage = ms.ToArray();
                        }
                    }
                }

                OnPropertyChanged("ReceivingVisibility");
            });
        }

        private static Dictionary<string, string> areas = Resource.epsp_area.Split('\n').Skip(1).Select(e => e.Split(',')).ToDictionary(e => e[0], e => e[4]);

        private static string ConfidenceLabel(double confidence)
        {
            return confidence switch
            {
                var n when n > 0.8 => "A",
                var n when n > 0.6 => "B",
                var n when n > 0.4 => "C",
                var n when n > 0.2 => "D",
                var n when n > 0.0 => "E",
                _ => "F"
            };
        }

        private Color ConvConfidenceColor(double confidence)
        {
            if (confidence < 0)
            {
                return Color.FromArgb(128, 192, 192, 192);
            }

            if (confidence >= 0.5)
            {
                var multiply = (confidence - 0.5) * 2;
                return Color.FromArgb(
                    255,
                    (byte)(244 + (multiply * -4)),
                    (byte)(160 + (multiply * -32)),
                    (byte)(64 + (multiply * -64))
                    );
            }
            else
            {
                var multiply = confidence * 2;
                return Color.FromArgb(
                        192,
                        (byte)(255 + (multiply * -11)),
                        (byte)(248 + (multiply * -88)),
                        (byte)(240 + (multiply * -176))
                    );
            }
        }

        private static IList<UserquakePoint> GenerateUserquakePoints(UserquakeEvaluateEventArgs eventArgs)
        {
            // 信頼度は正規化する（ただし、係数は 8 まで）
            var normalizationFactor = new double[] { 8, 1.0 / eventArgs.AreaConfidences.Select(e => e.Value.Confidence).Append(0.1).Max() }.Min();
            return eventArgs.AreaConfidences.Where(e => e.Value.Confidence > 0).Select(e => new UserquakePoint(e.Value.AreaCode, e.Value.Confidence * normalizationFactor)).ToList();
        }

        // See: https://docs.microsoft.com/ja-jp/dotnet/desktop/wpf/data/how-to-implement-property-change-notification
        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
