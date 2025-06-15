using Avalonia.Data.Converters;

using System;
using System.Collections.ObjectModel;

namespace AvaloniaUIClient.ViewModels
{
    public class InformationViewModel : ViewModelBase
    {
        public ObservableCollection<EventArgs> Histories { get; } = [];

        public double BodyWidth { get; set; }
        public double BodyHeight { get; set; }

        private EventArgs? _activeEventArgs;
        public EventArgs? ActiveEventArgs
        {
            get { return _activeEventArgs; }
            set
            {
                _activeEventArgs = value;
                OnPropertyChanged(nameof(ActiveEventArgs));
            }
        }

        private ViewModelBase? _activeViewModel;
        public ViewModelBase? ActiveViewModel
        {
            get { return _activeViewModel; }
            set
            {
                _activeViewModel = value;
                OnPropertyChanged(nameof(ActiveViewModel));
            }
        }
    }

    public static class ViewConverters
    {
        public static FuncValueConverter<string, string> ScaleConverter = new(value =>
        {
            if (value == "3以上" || value == "" || value == "-1") { return "不明"; }
            return value;
        });
    }
}
