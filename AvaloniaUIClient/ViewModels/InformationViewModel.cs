using Avalonia.Data.Converters;

using Client.Peer;

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
        // FIXME: 噴火に伴う情報に未対応
        public static FuncValueConverter<QuakeInformationType, string> QuakeTypeConverter = new(value => value switch
        {
            QuakeInformationType.ScalePrompt => "震度速報",
            QuakeInformationType.Destination => "震源情報",
            QuakeInformationType.ScaleAndDestination => "震度・震源情報",
            QuakeInformationType.Detail => "地震情報",
            QuakeInformationType.Foreign => "遠地（海外）地震情報",
            _ => "不明な地震情報",
        });
    }
}
