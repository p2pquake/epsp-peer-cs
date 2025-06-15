using Avalonia.Media.Imaging;

using Client.Peer;

using Map.Controller;

using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AvaloniaUIClient.ViewModels.Information
{
    public class EEWViewModel : ViewModelBase
    {
        private EPSPEEWEventArgs eventArgs;
        private InformationViewModel vm;

        public EEWViewModel(EPSPEEWEventArgs eventArgs, InformationViewModel vm)
        {
            this.eventArgs = eventArgs;
            this.vm = vm;

            _ = Task.Run(() =>
            {
                if (eventArgs == null) { return; }
                if (eventArgs.IsCancelled) { return; }

                var mapDrawer = new MapDrawer()
                {
                    MapType = Map.Model.MapType.JAPAN_4096,
                    Trim = true,
                    EEWPoints = eventArgs.Areas.Select(i => new Map.Model.EEWPoint(i.ToString())).ToArray(),
                    HideNote = true,
                    PreferedAspectRatio = vm.BodyWidth / vm.BodyHeight,
                };
                var png = mapDrawer.DrawAsPng();

                using var ms = new MemoryStream();
                png.CopyTo(ms);
                PngImage = ms.ToArray();
            });
        }

        public string EventTimeLabel { get => $"{eventArgs.ReceivedAt:dd日HH時mm分ss秒fff}"; }

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
}
