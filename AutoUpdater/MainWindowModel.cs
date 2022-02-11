using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AutoUpdater
{
    public enum UpdateStatus
    {
        Confirmation,
        Updating,
        Updated,
    }

    public class MainWindowModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public Visibility ConfirmationVisibility => updateStatus == UpdateStatus.Confirmation ? Visibility.Visible : Visibility.Hidden;
        public Visibility UpdateVisibility => updateStatus != UpdateStatus.Confirmation ? Visibility.Visible : Visibility.Hidden;
        public Visibility UpdatingVisibility => updateStatus == UpdateStatus.Updating ? Visibility.Visible : Visibility.Hidden;
        public Visibility UpdatedVisibility => updateStatus == UpdateStatus.Updated ? Visibility.Visible : Visibility.Hidden;

        private UpdateStatus updateStatus = UpdateStatus.Confirmation;
        public UpdateStatus UpdateStatus
        {
            get => updateStatus;
            set
            {
                updateStatus = value;
                OnPropertyChanged();
                OnPropertyChanged("ConfirmationVisibility");
                OnPropertyChanged("UpdateVisibility");
                OnPropertyChanged("UpdatingVisibility");
                OnPropertyChanged("UpdatedVisibility");
            }
        }

        private string updateMessage = "アップデートしています...";
        public string UpdateMessage
        {
            get => updateMessage;
            set
            {
                updateMessage = value;
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
