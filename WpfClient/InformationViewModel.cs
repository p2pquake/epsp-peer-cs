using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WpfClient
{
    public class SampleItem
    {
        public string MaxScale { get; set; }
        public string OccuredTime { get; set; }
        public string Destination { get; set; }
    }

    public class ForeignItem
    {
        public string OccuredTime { get; set; }
        public string Destination { get; set; }
    }

    public class InformationViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<object> SampleItems { get; } = new()
        {
            new SampleItem() { MaxScale = "2", OccuredTime = "24日14時19分", Destination = "鳥取県西部" },
            new SampleItem() { MaxScale = "1", OccuredTime = "24日13時44分", Destination = "宮城県沖" },
            new SampleItem() { MaxScale = "5弱", OccuredTime = "16日18時42分", Destination = "石川県能登地方" },
            new ForeignItem() { OccuredTime = "8日10時48分", Destination = "メキシコ、ゲレロ州" },
            new ForeignItem() { OccuredTime = "8月23日6時33分", Destination = "サウスサンドウィッチ諸島" },
        };

        // See: https://docs.microsoft.com/ja-jp/dotnet/desktop/wpf/data/how-to-implement-property-change-notification
        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
