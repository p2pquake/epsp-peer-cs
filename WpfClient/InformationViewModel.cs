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
using System.Windows;

using WpfClient.EPSPDataView;

namespace WpfClient
{
    public class InformationViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private object selectItem;
        public object SelectItem
        {
            get { return selectItem; }
            set {
                selectItem = value;
                OnPropertyChanged();
                OnPropertyChanged("EarthquakeVisibility");
                OnPropertyChanged("UserquakeVisibility");
            }
        }

        public Visibility EarthquakeVisibility =>
            SelectItem is EPSPQuakeView ? Visibility.Visible : Visibility.Hidden;
        public Visibility UserquakeVisibility =>
            SelectItem is EPSPUserquakeView ? Visibility.Visible : Visibility.Hidden;

        public ObservableCollection<object> Histories { get; } = new()
        {
            null,
            Factory.WrapEventArgs(new EPSPQuakeEventArgs()
            {
                InformationType = QuakeInformationType.Detail,
                OccuredTime = "24日13時44分",
                Scale = "1",
                Destination = "宮城県沖",
                Depth = "50km",
                Magnitude = "3.7",
                Latitude = "N38.2",
                Longitude = "E141.7",
                TsunamiType = DomesticTsunamiType.None,
                PointList = new List<QuakeObservationPoint>()
            {
                new QuakeObservationPoint() { Prefecture = "宮城県", Name = "大崎市", Scale = "1" },
                new QuakeObservationPoint() { Prefecture = "宮城県", Name = "岩沼市", Scale = "1" },
                new QuakeObservationPoint() { Prefecture = "宮城県", Name = "石巻市", Scale = "1" },
                new QuakeObservationPoint() { Prefecture = "福島県", Name = "福島伊達市", Scale = "1" },
            }
            }),
            Factory.WrapEventArgs(new EPSPQuakeEventArgs() { InformationType = QuakeInformationType.Detail, OccuredTime = "16日18時42分", Scale = "5弱", Destination = "石川県能登地方", Depth = "10km", Magnitude = "5.2", Latitude = "N37.5", Longitude = "E137.3", TsunamiType = DomesticTsunamiType.None }),
            Factory.WrapEventArgs(new EPSPQuakeEventArgs() { InformationType = QuakeInformationType.Destination, OccuredTime = "16日18時42分", Scale = "3以上", Destination = "石川県能登地方", Depth = "10km", Magnitude = "5.2", Latitude = "N37.5", Longitude = "E137.3", TsunamiType = DomesticTsunamiType.None }),
            Factory.WrapEventArgs(new EPSPQuakeEventArgs()
            {
                InformationType = QuakeInformationType.ScalePrompt,
                OccuredTime = "16日18時42分",
                Scale = "5弱",
                Destination = "",
                Depth = "",
                Magnitude = "",
                Latitude = "",
                Longitude = "",
                TsunamiType = DomesticTsunamiType.Checking,
                PointList = new List<QuakeObservationPoint>()
                {
                    new QuakeObservationPoint() { Prefecture = "石川県", Name = "石川県能登", Scale = "5弱" },
                    new QuakeObservationPoint() { Prefecture = "新潟県", Name = "新潟県上越", Scale = "3" },
                    new QuakeObservationPoint() { Prefecture = "新潟県", Name = "新潟県中越", Scale = "3" },
                    new QuakeObservationPoint() { Prefecture = "長野県", Name = "長野県北部", Scale = "3" },
                }
            }),
            Factory.WrapEventArgs(new EPSPQuakeEventArgs() { InformationType = QuakeInformationType.Foreign, OccuredTime = "8日10時48分", Scale = "不明", Destination = "メキシコ、ゲレロ州", Depth = "-1km", Magnitude = "7.4", Latitude = "N17.1", Longitude = "W99.6", TsunamiType = DomesticTsunamiType.None }),
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
            Factory.WrapEventArgs(new UserquakeEvaluateEventArgs() { StartedAt = DateTime.Parse("2021/09/24 23:55:45"), UpdatedAt = DateTime.Parse("2021/09/24 23:57:17"), Count = 33, Confidence = 0.98052, AreaConfidences = new Dictionary<string, IUserquakeEvaluationArea>()
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
