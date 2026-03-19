using FluentAvalonia.UI.Controls;

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AvaloniaClient.ViewModels;

public class RootViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public InformationViewModel InformationViewModel { get; } = new();
    public SettingViewModel SettingViewModel { get; } = new();
    public StatusViewModel StatusViewModel { get; } = new();

    private object currentPage;
    public object CurrentPage
    {
        get { return currentPage; }
        set
        {
            currentPage = value;
            OnPropertyChanged();
        }
    }

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

    private Symbol statusSymbol = Symbol.WifiWarning;
    public Symbol StatusSymbol
    {
        get { return statusSymbol; }
        set
        {
            statusSymbol = value;
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

    public RootViewModel()
    {
        currentPage = InformationViewModel;
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
