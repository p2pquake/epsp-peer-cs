using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Windows;

namespace WpfClient
{
    public class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // See: https://docs.microsoft.com/ja-jp/dotnet/desktop/wpf/data/how-to-implement-property-change-notification
        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private Visibility _showInfo = Visibility.Hidden;
        public Visibility ShowInfo
        {
            get { return _showInfo; }
            set
            {
                _showInfo = value;
                OnPropertyChanged();
            }
        }

        private Visibility _showSetting = Visibility.Hidden;
        public Visibility ShowSetting
        {
            get { return _showSetting; }
            set
            {
                _showSetting = value;
                OnPropertyChanged();
            }
        }

        private Visibility _showStatus = Visibility.Hidden;
        public Visibility ShowStatus
        {
            get { return _showStatus; }
            set
            {
                _showStatus = value;
                OnPropertyChanged();
            }
        }
    }
}
