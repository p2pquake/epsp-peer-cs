using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WpfClient
{
    public class RootViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // public RootViewModel()
        // {
        //     BindingDataContext = statusViewModel;
        // }

        public InformationViewModel InformationViewModel { get; } = new();
        public SettingViewModel SettingViewModel { get; } = new();
        public StatusViewModel StatusViewModel { get; } = new();

        private string pageFileName = "Pages/Status.xaml";
        public string PageFileName
        {
            get { return pageFileName; }
            set
            {
                pageFileName = value;
                OnPropertyChanged();
            }
        }

        public object BindingDataContext { get; set; }

        private bool informationIsSelected = true;
        public bool InformationIsSelected
        {
            get { return informationIsSelected; }
            set
            {
                informationIsSelected = value;
                OnPropertyChanged();
            }
        }

        private string status = "未接続";
        public string Status
        {
            get { return status; }
            set
            {
                status = value;
                OnPropertyChanged();
            }
        }

        private string statusIconGlyph = "\xF384";
        public string StatusIconGlyph
        {
            get { return statusIconGlyph; }
            set
            {
                statusIconGlyph = value;
                OnPropertyChanged();
            }
        }

        private string numberOfPeersLabel = "- ピア";
        public string NumberOfPeersLabel
        {
            get { return numberOfPeersLabel; }
            set
            {
                numberOfPeersLabel = value;
                OnPropertyChanged();
            }
        }

        private string portStatus = "ポート: -";
        public string PortStatus
        {
            get { return portStatus; }
            set
            {
                portStatus = value;
                OnPropertyChanged();
            }
        }

        private bool canSendUserquake = false;
        public bool CanSendUserquake
        {
            get { return canSendUserquake; }
            set
            {
                canSendUserquake = value;
                OnPropertyChanged();
            }
        }

        // See: https://docs.microsoft.com/ja-jp/dotnet/desktop/wpf/data/how-to-implement-property-change-notification
        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
