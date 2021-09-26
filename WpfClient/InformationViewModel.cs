using Client.App.Userquake;
using Client.Peer;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using WpfClient.EPSPDataView;

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

        public ObservableCollection<object> Histories { get; } = new()
        {
            null,
            Factory.WrapEventArgs(new EPSPQuakeEventArgs() { InformationType = QuakeInformationType.Detail, OccuredTime = "24日13時44分", Scale = "1", Destination = "宮城県沖" }),
            Factory.WrapEventArgs(new EPSPQuakeEventArgs() { InformationType = QuakeInformationType.Detail, OccuredTime = "16日18時42分", Scale = "5弱", Destination = "石川県能登地方" }),
            Factory.WrapEventArgs(new EPSPQuakeEventArgs() { InformationType = QuakeInformationType.Destination, OccuredTime = "16日18時42分", Destination = "石川県能登地方" }),
            Factory.WrapEventArgs(new EPSPQuakeEventArgs() { InformationType = QuakeInformationType.ScalePrompt, OccuredTime = "16日18時42分", Scale = "5弱" }),
            Factory.WrapEventArgs(new EPSPQuakeEventArgs() { InformationType = QuakeInformationType.Foreign, OccuredTime = "xx日xx時xx分", Destination = "ニューメキシコ州（米国）／チワワ州（メキシコ）境" }),
            Factory.WrapEventArgs(new EPSPTsunamiEventArgs() { IsCancelled = true, ReceivedAt = DateTime.Parse("2016/11/22 12:53:00") }),
            Factory.WrapEventArgs(new EPSPTsunamiEventArgs() { IsCancelled = false, ReceivedAt = DateTime.Parse("2016/11/22 08:12:00"), RegionList = new List<TsunamiForecastRegion>() {
                new TsunamiForecastRegion() { IsImmediately = false, Category = TsunamiCategory.Warning, Region = "宮城県" },
                new TsunamiForecastRegion() { IsImmediately = false, Category = TsunamiCategory.Warning, Region = "福島県" },
                new TsunamiForecastRegion() { IsImmediately = false, Category = TsunamiCategory.Advisory, Region = "青森県太平洋沿岸" },
                new TsunamiForecastRegion() { IsImmediately = false, Category = TsunamiCategory.Advisory, Region = "岩手県" },
                new TsunamiForecastRegion() { IsImmediately = false, Category = TsunamiCategory.Advisory, Region = "茨城県" },
                new TsunamiForecastRegion() { IsImmediately = false, Category = TsunamiCategory.Advisory, Region = "千葉県九十九里・外房" },
                new TsunamiForecastRegion() { IsImmediately = false, Category = TsunamiCategory.Advisory, Region = "千葉県内房" },
                new TsunamiForecastRegion() { IsImmediately = false, Category = TsunamiCategory.Advisory, Region = "伊豆諸島" },
            } }),
            Factory.WrapEventArgs(new EPSPEEWTestEventArgs() { ReceivedAt = DateTime.Parse("2021/05/01 10:27:44") }),
            Factory.WrapEventArgs(new UserquakeEvaluateEventArgs() { StartedAt = DateTime.Parse("2021/09/24 23:55:45"), Count = 33, Confidence = 0.98052, AreaConfidences = new Dictionary<string, IUserquakeEvaluationArea>()
            {
                { "225", new UserquakeEvaluationArea() { AreaCode = "225", Count = 7, Confidence = 0.1606914 } },
                { "215", new UserquakeEvaluationArea() { AreaCode = "215", Count = 4, Confidence = 0.0702406 } },
                { "220", new UserquakeEvaluationArea() { AreaCode = "220", Count = 1, Confidence = 0.0236118 } },
            } }),
        };

        // See: https://docs.microsoft.com/ja-jp/dotnet/desktop/wpf/data/how-to-implement-property-change-notification
        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
