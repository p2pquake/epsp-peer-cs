using Avalonia.Media.Imaging;

using Client.App.Userquake;

using Map.Controller;
using Map.Model;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AvaloniaUIClient.ViewModels.Information
{
    public class UserquakeViewModel : ViewModelBase
    {
        private InformationViewModel vm;
        private UserquakeRedrawer redrawer;

        private UserquakeEvaluateEventArgs _eventArgs;
        public UserquakeEvaluateEventArgs EventArgs
        {
            get => _eventArgs;
            set
            {
                _eventArgs = value;
                OnPropertyChanged(nameof(EventTimeLabel));
                OnPropertyChanged(nameof(Count));
            }
        }

        public UserquakeViewModel(UserquakeEvaluateEventArgs eventArgs, InformationViewModel vm)
        {
            this._eventArgs = eventArgs;
            this.vm = vm;
            this.redrawer = new UserquakeRedrawer(
                60,
                (byte[] image) => PngImage = image,
                () => vm.BodyWidth / vm.BodyHeight
            );
            this.redrawer.Enqueue(eventArgs);
        }

        public string EventTimeLabel { get => $"{_eventArgs.StartedAt:dd日HH時mm分ss秒}～{_eventArgs.UpdatedAt:HH時mm分ss秒}"; }
        public string Count { get => $"{_eventArgs.Count}件"; }
        private byte[] pngImage = BehindResource.loading;
        private byte[] PngImage
        {
            get => pngImage;
            set
            {
                lock (pngImage) { pngImage = value; }
                OnPropertyChanged(nameof(BitmapImage));
            }
        }

        public Bitmap BitmapImage
        {
            get
            {
                lock (pngImage)
                {
                    return new Bitmap(new MemoryStream(pngImage));
                }
            }
        }
    }

    /// <summary>
    /// 地震感知情報 非同期地図生成
    /// </summary>
    public class UserquakeRedrawer
    {
        private ConcurrentQueue<UserquakeEvaluateEventArgs> drawingQueue = new ConcurrentQueue<UserquakeEvaluateEventArgs>();

        public UserquakeRedrawer(int timeoutSeconds, Action<byte[]> onImage, Func<double> funcPreferedAspectRatio)
        {
            _ = Task.Run(() =>
            {
                var updatedAt = DateTime.Now;
                var firstRendering = true;

                while (firstRendering || DateTime.Now.Subtract(updatedAt).TotalSeconds <= timeoutSeconds)
                {
                    firstRendering = false;

                    UserquakeEvaluateEventArgs? eventArgs = null;
                    while (!drawingQueue.IsEmpty) { drawingQueue.TryDequeue(out eventArgs); }

                    if (eventArgs != null)
                    {
                        var mapDrawer = new MapDrawer()
                        {
                            MapType = Map.Model.MapType.JAPAN_2048,
                            Trim = true,
                            UserquakePoints = GenerateUserquakePoints(eventArgs),
                            HideNote = true,
                            PreferedAspectRatio = funcPreferedAspectRatio(),

                        };
                        var png = mapDrawer.DrawAsPng();
                        using var ms = new MemoryStream();
                        png.CopyTo(ms);
                        onImage(ms.ToArray());
                    }

                    Thread.Sleep(250);
                }
            });
        }

        public void Enqueue(UserquakeEvaluateEventArgs eventArgs)
        {
            drawingQueue.Enqueue(eventArgs);
        }

        private static IList<UserquakePoint> GenerateUserquakePoints(UserquakeEvaluateEventArgs eventArgs)
        {
            // 信頼度は正規化する（ただし、係数は 8 まで）
            var normalizationFactor = new double[] { 8, 1.0 / eventArgs.AreaConfidences.Select(e => e.Value.Confidence).Append(0.1).Max() }.Min();
            return eventArgs.AreaConfidences.Where(e => e.Value.Confidence > 0).Select(e => new UserquakePoint(e.Value.AreaCode, e.Value.Confidence * normalizationFactor))
                .Where(e => e.Confidence > 0.01).ToList();
        }
    }
}
