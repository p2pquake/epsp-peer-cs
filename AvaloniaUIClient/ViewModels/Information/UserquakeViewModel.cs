using Avalonia.Media;
using Avalonia.Media.Imaging;
using AvaloniaUIClient.Models;
using AvaloniaUIClient.Utils;

using Client.App.Userquake;

using Map.Controller;
using Map.Model;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
                OnPropertyChanged(nameof(Rate));
                OnPropertyChanged(nameof(ReceivingVisibility));
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
        public string Rate 
        { 
            get 
            {
                if (_eventArgs == null) { return "-"; }

                var diff = _eventArgs.UpdatedAt.Subtract(_eventArgs.StartedAt).TotalSeconds;
                if (diff <= 0)
                {
                    return "計測中";
                }

                return $"{_eventArgs.Count / diff:N2}/sec";
            }
        }

        public bool ReceivingVisibility 
        { 
            get 
            {
                // 現在時刻と更新時刻の差が40秒未満の場合は受信中と判断
                return _eventArgs != null && DateTime.Now.Subtract(_eventArgs.UpdatedAt).TotalSeconds < 40;
            }
        }

        public ObservableCollection<UserquakeAreaDetail> AreaDetails
        {
            get
            {
                var details = new ObservableCollection<UserquakeAreaDetail>();
                
                if (_eventArgs?.AreaConfidences == null)
                    return details;

                var userquakePoints = GenerateUserquakePoints(_eventArgs)
                    .Where(e => AreaHelper.Areas.ContainsKey(e.Areacode))
                    .OrderByDescending(e => e.Confidence)
                    .ToList();

                foreach (var point in userquakePoints)
                {
                    var count = _eventArgs.AreaConfidences[point.Areacode].Count;
                    var confidence = point.Confidence * 80; // 信頼度バー幅（最大80px）
                    var areaName = AreaHelper.GetAreaName(point.Areacode);
                    var label = $"{areaName} ({count} 件)";

                    details.Add(new UserquakeAreaDetail
                    {
                        AreaName = label,
                        Count = count,
                        Confidence = confidence,
                        ConfidenceText = $"{confidence:F0}px",
                        brush = GetConfidenceBrush(point.Confidence)
                    });
                }

                return details;
            }
        }

        private static string GetAreaName(string areaCode)
        {
            return AreaHelper.GetAreaName(areaCode);
        }

        private static IBrush GetConfidenceBrush(double confidence)
        {
            var color = ConvConfidenceColor(confidence);
            return new SolidColorBrush(Color.FromArgb(color.A, color.R, color.G, color.B));
        }

        private static System.Drawing.Color ConvConfidenceColor(double confidence)
        {
            if (confidence < 0)
            {
                return System.Drawing.Color.FromArgb(128, 192, 192, 192);
            }

            if (confidence >= 0.5)
            {
                var multiply = (confidence - 0.5) * 2;
                return System.Drawing.Color.FromArgb(
                    255,
                    (byte)(244 + (multiply * -4)),
                    (byte)(160 + (multiply * -32)),
                    (byte)(64 + (multiply * -64))
                    );
            }
            else
            {
                var multiply = confidence * 2;
                return System.Drawing.Color.FromArgb(
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
            return eventArgs.AreaConfidences.Where(e => e.Value.Confidence > 0).Select(e => new UserquakePoint(e.Value.AreaCode, e.Value.Confidence * normalizationFactor))
                .Where(e => e.Confidence > 0.01).ToList();
        }
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
