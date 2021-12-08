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
            }
        }

        public IFrameModel FrameModel { get; init; }

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

                    var bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.CreateOptions = BitmapCreateOptions.None;
                    bitmapImage.StreamSource = new MemoryStream(pngImage);
                    bitmapImage.EndInit();
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
                            MapType = Map.Model.MapType.JAPAN_4096,
                            Trim = true,
                            UserquakePoints = GenerateUserquakePoints(eventArgs),
                            HideNote = true,
                            PreferedAspectRatio = FrameModel.FrameWidth / FrameModel.FrameHeight,
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

        private static IList<UserquakePoint> GenerateUserquakePoints(UserquakeEvaluateEventArgs eventArgs)
        {
            return eventArgs.AreaConfidences.Select(e => new UserquakePoint(e.Value.AreaCode, e.Value.Confidence)).ToList();
        }

        // See: https://docs.microsoft.com/ja-jp/dotnet/desktop/wpf/data/how-to-implement-property-change-notification
        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
