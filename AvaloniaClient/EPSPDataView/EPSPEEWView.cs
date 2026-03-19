using Avalonia.Media.Imaging;

using Client.Peer;

using Map.Controller;
using Map.Model;

using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using AvaloniaClient.Utils;
using AvaloniaClient.ViewModels;

namespace AvaloniaClient.EPSPDataView;

public class EPSPEEWView : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public EPSPEEWEventArgs EventArgs { get; init; } = null!;
    public IFrameModel FrameModel { get; init; } = null!;

    public string Time => EventArgs.ReceivedAt.ToString("dd日HH時mm分");
    public string DetailTime => EventArgs?.ReceivedAt.ToString("dd日HH時mm分ss秒fff") ?? "";
    public bool TestLabelVisible => EventArgs?.Areas.Contains(-1) ?? false;
    public bool IsCancelled => EventArgs?.IsCancelled ?? false;
    public bool IsNotCancelled => !IsCancelled;
    public Avalonia.Media.IBrush EEWListForeground => IsCancelled ? Avalonia.Media.Brushes.Gray : Avalonia.Media.Brushes.Red;

    private byte[]? pngImage;
    private readonly object pngLock = new();

    public Bitmap? BitmapImage
    {
        get
        {
            lock (pngLock)
            {
                return BitmapHelper.FromBytes(pngImage);
            }
        }
    }

    public string Hypocenter
    {
        get
        {
            if (EventArgs?.Hypocenter != null && EEWConverter.GetHypocenter(EventArgs.Hypocenter) != null)
                return $"{EEWConverter.GetHypocenter(EventArgs.Hypocenter)} で地震";
            return "";
        }
    }

    public string Areas
    {
        get
        {
            if (EventArgs?.Areas == null) return "（不明）";
            var areaNames = EventArgs.Areas.Select(area => EEWConverter.GetArea(area) ?? "（不明）");
            return string.Join('　', areaNames);
        }
    }

    public string Caption
    {
        get
        {
            if (EventArgs.IsCancelled) return "緊急地震速報（警報） 取消";
            if (EventArgs.IsFollowUp) return "緊急地震速報（警報） 続報";
            return "緊急地震速報（警報）";
        }
    }

    public EPSPEEWView() { }

    public EPSPEEWView(EPSPEEWEventArgs eventArgs, IFrameModel frameModel)
    {
        EventArgs = eventArgs;
        FrameModel = frameModel;

        _ = Task.Run(() =>
        {
            if (EventArgs == null || EventArgs.IsCancelled) return;

            var mapDrawer = new MapDrawer()
            {
                MapType = MapType.JAPAN_4096,
                Trim = true,
                EEWPoints = EventArgs.Areas.Select(i => new EEWPoint(i.ToString())).ToArray(),
                HideNote = true,
                PreferedAspectRatio = (FrameModel.FrameWidth / 2) / (FrameModel.FrameHeight - 40),
            };
            var png = mapDrawer.DrawAsPng();

            using var ms = new MemoryStream();
            png.CopyTo(ms);
            lock (pngLock)
            {
                pngImage = ms.ToArray();
            }
            OnPropertyChanged("BitmapImage");
        });
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
