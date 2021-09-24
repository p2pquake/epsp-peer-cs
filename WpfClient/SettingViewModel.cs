using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfClient
{
    public class SettingViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string selectTag;
        public string SelectTag
        {
            get { return selectTag; }
            set
            {
                selectTag = value;
                OnPropertyChanged("BootVisibility");
                OnPropertyChanged("ConnectionVisibility");
                OnPropertyChanged("UserquakeVisibility");
                OnPropertyChanged("NotificationVisibility");
            }
        }

        public Visibility BootVisibility
        {
            get { return selectTag == "Boot" ? Visibility.Visible : Visibility.Hidden; }
        }

        public Visibility ConnectionVisibility
        {
            get { return selectTag == "Connection" ? Visibility.Visible : Visibility.Hidden; }
        }

        public Visibility UserquakeVisibility
        {
            get { return selectTag == "Userquake" ? Visibility.Visible : Visibility.Hidden; }
        }

        public Visibility NotificationVisibility
        {
            get { return selectTag == "Notification" ? Visibility.Visible : Visibility.Hidden; }
        }

        public bool BootAtStartup { get; set; }
        public bool MinimizeAtBoot { get; set; }

        private bool portOpen;
        public bool PortOpen
        {
            get { return portOpen; }
            set
            {
                portOpen = value;
                OnPropertyChanged();
            }
        }
        public bool UseUPnP { get; set; }
        public int Port { get; set; }

        // See: https://docs.microsoft.com/ja-jp/dotnet/desktop/wpf/data/how-to-implement-property-change-notification
        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
