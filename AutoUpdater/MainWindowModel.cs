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
        Checking,
        Confirmation,
        Updating,
        Updated,
    }

    public class MainWindowModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public Visibility CheckingVisibility => CalcVisibility(UpdateStatus.Checking);
        public Visibility ConfirmationVisibility => CalcVisibility(UpdateStatus.Confirmation);
        public Visibility UpdatingVisibility => CalcVisibility(UpdateStatus.Updating);
        public Visibility UpdatedVisibility => CalcVisibility(UpdateStatus.Updated);

        private Visibility CalcVisibility(UpdateStatus desiredStatus) => updateStatus == desiredStatus ? Visibility.Visible : Visibility.Hidden;

        private UpdateStatus updateStatus = UpdateStatus.Checking;
        public UpdateStatus UpdateStatus
        {
            get => updateStatus;
            set
            {
                updateStatus = value;
                OnPropertyChanged();
                OnPropertyChanged("CheckingVisibility");
                OnPropertyChanged("ConfirmationVisibility");
                OnPropertyChanged("UpdatingVisibility");
                OnPropertyChanged("UpdatedVisibility");
            }
        }

        private string updatedResultMessage = "";
        public string UpdatedResultMessage
        {
            get => updatedResultMessage;
            set
            {
                updatedResultMessage = value;
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
