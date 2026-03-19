using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;

using Client.App.Userquake;

using Map.Controller;
using Map.Model;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using AvaloniaClient.Utils;
using AvaloniaClient.ViewModels;

namespace AvaloniaClient.EPSPDataView;

public class EPSPUserquakeView : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private ConcurrentQueue<UserquakeEvaluateEventArgs> drawingQueue = new();

    private UserquakeEvaluateEventArgs eventArgs = null!;
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

    public Func<DateTime> ProtocolTime { get; init; } = null!;
    public IFrameModel FrameModel { get; init; } = null!;

    public bool TestLabelVisible => EventArgs?.AreaConfidences.Any((area) => area.Key == "-1") ?? false;

    public string Source => Application.Current?.ActualThemeVariant == ThemeVariant.Dark
        ? "avares://P2PQuake/Resources/Icons/userquake_white.png"
        : "avares://P2PQuake/Resources/Icons/userquake_black.png";

    public string Time => EventArgs.StartedAt.ToString("dd日HH時mm分");
    public string Caption => "地震感知情報";

    public bool ReceivingVisible =>
        EventArgs != null && ProtocolTime().Subtract(EventArgs.UpdatedAt).TotalSeconds < 40;

    public string DetailTime => $"{EventArgs?.StartedAt.ToString("M月dd日HH時mm分ss秒")}～{EventArgs?.UpdatedAt.ToString("HH時mm分ss秒")}";

    public string Count => EventArgs == null ? "不明" : $"{EventArgs.Count} 件";

    public string Rate
    {
        get
        {
            if (EventArgs == null) return "-";
            var diff = EventArgs.UpdatedAt.Subtract(EventArgs.StartedAt).TotalSeconds;
            if (diff <= 0) return "計測中";
            return $"{EventArgs.Count / diff:N2}/sec";
        }
    }

    public record UserquakeDetail(double Confidence, string Label, IBrush Brush);

    public List<UserquakeDetail> UserquakeDetails
    {
        get
        {
            if (EventArgs == null) return new();
            return GenerateUserquakePoints(EventArgs)
                .Where(e => areas.ContainsKey(e.Areacode))
                .OrderByDescending(e => e.Confidence)
                .Select(e => new UserquakeDetail(
                    e.Confidence * 80,
                    $"{areas[e.Areacode]} ({EventArgs.AreaConfidences[e.Areacode].Count} 件)",
                    new SolidColorBrush(ConvConfidenceColor(e.Confidence))))
                .ToList();
        }
    }

    private byte[]? pngImage;
    private readonly object pngLock = new();

    public Bitmap? BitmapImage
    {
        get
        {
            lock (pngLock)
            {
                if (pngImage == null) return null;
                if (eventArgs != null && !ReceivingVisible)
                {
                    var mapDrawer = new MapDrawer()
                    {
                        MapType = MapType.JAPAN_2048,
                        Trim = true,
                        UserquakePoints = GenerateUserquakePoints(EventArgs),
                        HideNote = true,
                        PreferedAspectRatio = (FrameModel.FrameWidth - 240) / (FrameModel.FrameHeight - 40),
                    };
                    var png = mapDrawer.DrawAsPng();
                    using var ms = new MemoryStream();
                    png.CopyTo(ms);
                    pngImage = ms.ToArray();
                }
                return BitmapHelper.FromBytes(pngImage);
            }
        }
    }

    public EPSPUserquakeView() { }

    public EPSPUserquakeView(UserquakeEvaluateEventArgs eventArgs, Func<DateTime> protocolTime, IFrameModel frameModel)
    {
        this.ProtocolTime = protocolTime;
        this.FrameModel = frameModel;
        this.EventArgs = eventArgs;

        _ = Task.Run(() =>
        {
            var firstRendering = true;
            while (firstRendering || ReceivingVisible)
            {
                firstRendering = false;
                Thread.Sleep(250);

                UserquakeEvaluateEventArgs? latest = null;
                while (!drawingQueue.IsEmpty) { drawingQueue.TryDequeue(out latest); }

                if (latest != null)
                {
                    var mapDrawer = new MapDrawer()
                    {
                        MapType = MapType.JAPAN_2048,
                        Trim = true,
                        UserquakePoints = GenerateUserquakePoints(latest),
                        HideNote = true,
                        PreferedAspectRatio = (FrameModel.FrameWidth - 240) / (FrameModel.FrameHeight - 40),
                    };
                    var png = mapDrawer.DrawAsPng();
                    using var ms = new MemoryStream();
                    png.CopyTo(ms);
                    lock (pngLock)
                    {
                        pngImage = ms.ToArray();
                    }
                    OnPropertyChanged("BitmapImage");
                }
            }
            OnPropertyChanged("ReceivingVisible");
        });
    }

    private static Dictionary<string, string> areas = AreaDataProvider.AreaDictionary;

    private Color ConvConfidenceColor(double confidence)
    {
        if (confidence < 0) return Color.FromArgb(128, 192, 192, 192);
        if (confidence >= 0.5)
        {
            var multiply = (confidence - 0.5) * 2;
            return Color.FromArgb(255, (byte)(244 + (multiply * -4)), (byte)(160 + (multiply * -32)), (byte)(64 + (multiply * -64)));
        }
        else
        {
            var multiply = confidence * 2;
            return Color.FromArgb(192, (byte)(255 + (multiply * -11)), (byte)(248 + (multiply * -88)), (byte)(240 + (multiply * -176)));
        }
    }

    private static IList<UserquakePoint> GenerateUserquakePoints(UserquakeEvaluateEventArgs eventArgs)
    {
        var normalizationFactor = new double[] { 8, 1.0 / eventArgs.AreaConfidences.Select(e => e.Value.Confidence).Append(0.1).Max() }.Min();
        return eventArgs.AreaConfidences.Where(e => e.Value.Confidence > 0).Select(e => new UserquakePoint(e.Value.AreaCode, e.Value.Confidence * normalizationFactor))
            .Where(e => e.Confidence > 0.01).ToList();
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
