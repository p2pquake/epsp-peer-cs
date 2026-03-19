using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using AvaloniaClient.EPSPDataView;

namespace AvaloniaClient.ViewModels;

public interface IFrameModel
{
    public double FrameWidth { get; set; }
    public double FrameHeight { get; set; }
}

public class InformationViewModel : INotifyPropertyChanged, IFrameModel
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public double FrameWidth { get; set; }
    public double FrameHeight { get; set; }

    private bool isLoading = true;
    public bool IsLoading
    {
        get { return isLoading; }
        set
        {
            isLoading = value;
            OnPropertyChanged();
        }
    }

    private bool tutorialVisible = false;
    public bool TutorialVisible
    {
        get { return tutorialVisible; }
        set
        {
            tutorialVisible = value;
            OnPropertyChanged();
        }
    }

    private object? selectItem;
    public object? SelectItem
    {
        get { return selectItem; }
        set
        {
            selectItem = value;
            OnPropertyChanged();
        }
    }

    private int selectedIndex;
    public int SelectedIndex
    {
        get { return selectedIndex; }
        set
        {
            selectedIndex = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<object> Histories { get; } = new();

    private void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
