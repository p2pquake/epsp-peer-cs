using Client.Peer;

using Map.Controller;
using Map.Model;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

using WpfClient.Utils;

namespace WpfClient.EPSPDataView
{
    public class EPSPEEWView : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public EPSPEEWEventArgs EventArgs { get; init; }
        public IFrameModel FrameModel { get; init; }

        public string Time => EventArgs.ReceivedAt.ToString("dd日HH時mm分");
        public string DetailTime => EventArgs?.ReceivedAt.ToString("dd日HH時mm分ss秒fff");

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
                    bitmapImage.StreamSource = new MemoryStream(pngImage);
                    bitmapImage.EndInit();
                    bitmapImage.Freeze();
                    GC.Collect();
                    return bitmapImage;
                }
            }
        }

        public string Hypocenter
        {
            get
            {
                if (EventArgs?.Hypocenter != null && EEWConverter.GetHypocenter(EventArgs.Hypocenter) != null)
                {
                    return $"{EEWConverter.GetHypocenter(EventArgs.Hypocenter)} で地震";
                }
                return "";
            }
        }

        public string Areas
        {
            get
            {
                if (EventArgs?.Areas == null)
                {
                    return "（不明）";
                }

                var areaNames =
                    EventArgs.Areas.Select(area =>
                    {
                        return EEWConverter.GetArea(area) ?? "（不明）";
                    });

                return string.Join('　', areaNames);
            }
        }

        public EPSPEEWView()
        {

        }

        public EPSPEEWView(EPSPEEWEventArgs eventArgs, IFrameModel frameModel)
        {
            EventArgs = eventArgs;
            FrameModel = frameModel;

            // 画像生成スレッド
            _ = Task.Run(() =>
            {
                if (EventArgs == null) { return; }

                var mapDrawer = new MapDrawer()
                {
                    MapType = Map.Model.MapType.JAPAN_4096,
                    Trim = true,
                    EEWPoints = EventArgs.Areas.Select(i => new Map.Model.EEWPoint(i.ToString())).ToArray(),
                    HideNote = true,
                    PreferedAspectRatio = (FrameModel.FrameWidth / 2) / (FrameModel.FrameHeight - 40),
                };
                var png = mapDrawer.DrawAsPng();

                using (var ms = new MemoryStream())
                {
                    png.CopyTo(ms);
                    PngImage = ms.ToArray();
                }
            });
        }




        // See: https://docs.microsoft.com/ja-jp/dotnet/desktop/wpf/data/how-to-implement-property-change-notification
        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
