using Client.App;

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

using AvaloniaClient.Services;
using AvaloniaClient.Utils;

namespace AvaloniaClient.ViewModels;

public class SettingViewModel : INotifyPropertyChanged
{
    public MediatorContext? MediatorContext { get; set; }
    public event PropertyChangedEventHandler? PropertyChanged;

    private readonly string[][] earthquakeScaleTable = new string[][]
    {
        new string[] { "10", "1" },
        new string[] { "20", "2" },
        new string[] { "30", "3" },
        new string[] { "40", "4" },
        new string[] { "45", "5弱" },
        new string[] { "50", "5強" },
        new string[] { "55", "6弱" },
        new string[] { "60", "6強" },
        new string[] { "70", "7" },
    };

    private string? selectTag;
    public string? SelectTag
    {
        get { return selectTag; }
        set
        {
            selectTag = value;
            OnPropertyChanged("BootVisible");
            OnPropertyChanged("ConnectionVisible");
            OnPropertyChanged("UserquakeVisible");
            OnPropertyChanged("NotificationVisible");
        }
    }

    public bool BootVisible => selectTag == "Boot";
    public bool ConnectionVisible => selectTag == "Connection";
    public bool UserquakeVisible => selectTag == "Userquake";
    public bool NotificationVisible => selectTag == "Notification";

    private bool bootAtStartup;
    public bool BootAtStartup
    {
        get => bootAtStartup;
        set { bootAtStartup = value; OnPropertyChanged(); }
    }

    private bool minimizeAtBoot;
    public bool MinimizeAtBoot
    {
        get => minimizeAtBoot;
        set { minimizeAtBoot = value; OnPropertyChanged(); }
    }

    private bool portOpen;
    public bool PortOpen
    {
        get => portOpen;
        set { portOpen = value; OnPropertyChanged(); }
    }

    private bool useUPnP;
    public bool UseUPnP
    {
        get => useUPnP;
        set { useUPnP = value; OnPropertyChanged(); }
    }

    private int port;
    public int Port
    {
        get => port;
        set { port = value; OnPropertyChanged(); }
    }

    private bool disconnectionComplement;
    public bool DisconnectionComplement
    {
        get => disconnectionComplement;
        set { disconnectionComplement = value; OnPropertyChanged(); }
    }

    public ObservableCollection<string> UserquakeAreas => new(AreaDataProvider.AreaLabels);

    private string? selectArea;
    public string? SelectArea
    {
        get => selectArea;
        set { selectArea = value; OnPropertyChanged(); }
    }

    private bool sendIfMiddleDoubleClick;
    public bool SendIfMiddleDoubleClick
    {
        get => sendIfMiddleDoubleClick;
        set { sendIfMiddleDoubleClick = value; OnPropertyChanged(); }
    }

    private bool sendIfRightDoubleClick;
    public bool SendIfRightDoubleClick
    {
        get => sendIfRightDoubleClick;
        set { sendIfRightDoubleClick = value; OnPropertyChanged(); }
    }

    private bool earthquakeNotification;
    public bool EarthquakeNotification
    {
        get => earthquakeNotification;
        set { earthquakeNotification = value; OnPropertyChanged(); }
    }

    public ObservableCollection<string> EarthquakeScales => new(earthquakeScaleTable.Select(e => $"震度 {e[1]} 以上"));

    private string? earthquakeMinScale;
    public string? EarthquakeMinScale
    {
        get => earthquakeMinScale;
        set { earthquakeMinScale = value; OnPropertyChanged(); }
    }

    private bool earthquakeForeign;
    public bool EarthquakeForeign
    {
        get => earthquakeForeign;
        set { earthquakeForeign = value; OnPropertyChanged(); }
    }

    private bool earthquakeShow;
    public bool EarthquakeShow
    {
        get => earthquakeShow;
        set { earthquakeShow = value; OnPropertyChanged(); }
    }

    private bool earthquakeNotice;
    public bool EarthquakeNotice
    {
        get => earthquakeNotice;
        set { earthquakeNotice = value; OnPropertyChanged(); }
    }

    private bool earthquakeSound;
    public bool EarthquakeSound
    {
        get => earthquakeSound;
        set { earthquakeSound = value; OnPropertyChanged(); }
    }

    private bool userquakeNotification;
    public bool UserquakeNotification
    {
        get => userquakeNotification;
        set { userquakeNotification = value; OnPropertyChanged(); }
    }

    private bool userquakeShow;
    public bool UserquakeShow
    {
        get => userquakeShow;
        set { userquakeShow = value; OnPropertyChanged(); }
    }

    private bool userquakeNotice;
    public bool UserquakeNotice
    {
        get => userquakeNotice;
        set { userquakeNotice = value; OnPropertyChanged(); }
    }

    private bool userquakeSound;
    public bool UserquakeSound
    {
        get => userquakeSound;
        set { userquakeSound = value; OnPropertyChanged(); }
    }

    private bool tsunamiNotification;
    public bool TsunamiNotification
    {
        get => tsunamiNotification;
        set { tsunamiNotification = value; OnPropertyChanged(); }
    }

    private bool tsunamiShow;
    public bool TsunamiShow
    {
        get => tsunamiShow;
        set { tsunamiShow = value; OnPropertyChanged(); }
    }

    private bool tsunamiNotice;
    public bool TsunamiNotice
    {
        get => tsunamiNotice;
        set { tsunamiNotice = value; OnPropertyChanged(); }
    }

    private bool tsunamiSound;
    public bool TsunamiSound
    {
        get => tsunamiSound;
        set { tsunamiSound = value; OnPropertyChanged(); }
    }

    private bool eewTestNotification;
    public bool EEWTestNotification
    {
        get => eewTestNotification;
        set { eewTestNotification = value; OnPropertyChanged(); }
    }

    private bool eewTestShow;
    public bool EEWTestShow
    {
        get => eewTestShow;
        set { eewTestShow = value; OnPropertyChanged(); }
    }

    private bool eewTestNotice;
    public bool EEWTestNotice
    {
        get => eewTestNotice;
        set { eewTestNotice = value; OnPropertyChanged(); }
    }

    private bool eewTestSound;
    public bool EEWTestSound
    {
        get => eewTestSound;
        set { eewTestSound = value; OnPropertyChanged(); }
    }

    private bool eewVoiceGuidance;
    public bool EEWVoiceGuidance
    {
        get => eewVoiceGuidance;
        set { eewVoiceGuidance = value; OnPropertyChanged(); }
    }

    public void LoadFromConfiguration(Configuration configuration)
    {
        bootAtStartup = configuration.BootAtStartup;
        minimizeAtBoot = configuration.MinimizeAtBoot;
        portOpen = configuration.PortOpen;
        useUPnP = configuration.UseUPnP;
        port = configuration.Port;
        disconnectionComplement = configuration.DisconnectionComplement;

        selectArea = ConvertAreaCodeToLabel(configuration.AreaCode);
        sendIfMiddleDoubleClick = configuration.SendIfMiddleDoubleClick;
        sendIfRightDoubleClick = configuration.SendIfRightDoubleClick;

        earthquakeNotification = configuration.EarthquakeNotification.Enabled;
        earthquakeMinScale = ConvertScaleValueToLabel(configuration.EarthquakeNotification.MinScale);
        earthquakeForeign = configuration.EarthquakeNotification.Foreign;
        earthquakeShow = configuration.EarthquakeNotification.Show;
        earthquakeNotice = configuration.EarthquakeNotification.Notice;
        earthquakeSound = configuration.EarthquakeNotification.Sound;

        userquakeNotification = configuration.UserquakeNotification.Enabled;
        userquakeShow = configuration.UserquakeNotification.Show;
        userquakeNotice = configuration.UserquakeNotification.Notice;
        userquakeSound = configuration.UserquakeNotification.Sound;

        tsunamiNotification = configuration.TsunamiNotification.Enabled;
        tsunamiShow = configuration.TsunamiNotification.Show;
        tsunamiNotice = configuration.TsunamiNotification.Notice;
        tsunamiSound = configuration.TsunamiNotification.Sound;

        eewTestNotification = configuration.EEWTestNotification.Enabled;
        eewTestShow = configuration.EEWTestNotification.Show;
        eewTestNotice = configuration.EEWTestNotification.Notice;
        eewTestSound = configuration.EEWTestNotification.Sound;
        eewVoiceGuidance = configuration.EEWTestNotification.VoiceGuidance;
    }

    private void WritebackConfiguration()
    {
        var configuration = ConfigurationManager.Configuration;

        configuration.BootAtStartup = bootAtStartup;
        configuration.MinimizeAtBoot = minimizeAtBoot;
        configuration.PortOpen = portOpen;
        configuration.UseUPnP = useUPnP;
        configuration.Port = port;
        configuration.DisconnectionComplement = disconnectionComplement;

        configuration.AreaCode = ConvertAreaToAreaCode(selectArea);
        configuration.SendIfMiddleDoubleClick = sendIfMiddleDoubleClick;
        configuration.SendIfRightDoubleClick = sendIfRightDoubleClick;

        configuration.EarthquakeNotification.Enabled = earthquakeNotification;
        configuration.EarthquakeNotification.MinScale = ConvertScaleLabelToValue(earthquakeMinScale);
        configuration.EarthquakeNotification.Foreign = earthquakeForeign;
        configuration.EarthquakeNotification.Show = earthquakeShow;
        configuration.EarthquakeNotification.Notice = earthquakeNotice;
        configuration.EarthquakeNotification.Sound = earthquakeSound;

        configuration.UserquakeNotification.Enabled = userquakeNotification;
        configuration.UserquakeNotification.Show = userquakeShow;
        configuration.UserquakeNotification.Notice = userquakeNotice;
        configuration.UserquakeNotification.Sound = userquakeSound;

        configuration.TsunamiNotification.Enabled = tsunamiNotification;
        configuration.TsunamiNotification.Show = tsunamiShow;
        configuration.TsunamiNotification.Notice = tsunamiNotice;
        configuration.TsunamiNotification.Sound = tsunamiSound;

        configuration.EEWTestNotification.Enabled = eewTestNotification;
        configuration.EEWTestNotification.Show = eewTestShow;
        configuration.EEWTestNotification.Notice = eewTestNotice;
        configuration.EEWTestNotification.Sound = eewTestSound;
        configuration.EEWTestNotification.VoiceGuidance = eewVoiceGuidance;

        ConfigurationManager.Save();
    }

    private int ConvertScaleLabelToValue(string? scaleLabel)
    {
        if (scaleLabel == null) return 10;
        var simplify = scaleLabel.Replace("震度", "").Replace("以上", "").Replace(" ", "");
        return int.Parse(earthquakeScaleTable.First(e => e[1] == simplify)[0]);
    }

    private string ConvertScaleValueToLabel(int scale)
    {
        var simplify = earthquakeScaleTable.First(e => int.Parse(e[0]) == scale)[1];
        return $"震度 {simplify} 以上";
    }

    private int ConvertAreaToAreaCode(string? areaLabel)
    {
        var areaCodeLabels = AreaDataProvider.AreaCodeLabels;
        var item = areaCodeLabels.FirstOrDefault(e => e.label == areaLabel);
        return item == default ? 900 : item.code;
    }

    private string ConvertAreaCodeToLabel(int areaCode)
    {
        var areaCodeLabels = AreaDataProvider.AreaCodeLabels;
        var item = areaCodeLabels.FirstOrDefault(e => e.code == areaCode);
        return item == default ? "未設定" : item.label;
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        WritebackConfiguration();
    }
}
