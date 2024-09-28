using Avalonia.Data.Converters;

using Client.Peer;

using System.Collections.ObjectModel;

namespace AvaloniaUIClient.ViewModels
{
    public class InformationViewModel : ViewModelBase
    {
        public ObservableCollection<object> Histories { get; } = new ObservableCollection<object>();
    }

    public static class ViewConverters
    {
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
