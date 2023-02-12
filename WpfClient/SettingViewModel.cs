using Client.App;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public MediatorContext MediatorContext { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        // FIXME: 震度の変換テーブルが Program.cs にもある。後で整理したい
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

        private readonly (int code, string label)[] areaCodeLabels =
            Resource.epsp_area.Split('\n').Skip(1).Select(e => e.Split(',')).Select(e => (int.Parse(e[1]), e[4])).ToArray();

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

        private bool bootAtStartup;
        public bool BootAtStartup
        {
            get => bootAtStartup;
            set
            {
                bootAtStartup = value;
                OnPropertyChanged();
            }
        }

        private bool minimizeAtBoot;
        public bool MinimizeAtBoot
        {
            get => minimizeAtBoot;
            set
            {
                minimizeAtBoot = value;
                OnPropertyChanged();
            }
        }

        private bool autoUpdate;
        public bool AutoUpdate
        {
            get => autoUpdate;
            set
            {
                autoUpdate = value;
                OnPropertyChanged();
            }
        }

        private bool portOpen;
        public bool PortOpen
        {
            get => portOpen;
            set
            {
                portOpen = value;
                OnPropertyChanged();
            }
        }

        private bool useUPnP;
        public bool UseUPnP
        {
            get => useUPnP;
            set
            {
                useUPnP = value;
                OnPropertyChanged();
            }
        }

        private int port;
        public int Port
        {
            get => port;
            set
            {
                port = value;
                OnPropertyChanged();
            }
        }

        private bool disconnectionComplement;
        public bool DisconnectionComplement
        {
            get => disconnectionComplement;
            set
            {
                disconnectionComplement = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> UserquakeAreas =>
            new ObservableCollection<string>(Resource.epsp_area.Split('\n').Skip(1).Select(e => e.Split(',')[4]));

        private string selectArea;
        public string SelectArea
        {
            get => selectArea;
            set
            {
                selectArea = value;
                OnPropertyChanged();
            }
        }

        private bool sendIfMiddleDoubleClick;
        public bool SendIfMiddleDoubleClick
        {
            get => sendIfMiddleDoubleClick;
            set
            {
                sendIfMiddleDoubleClick = value;
                OnPropertyChanged();
            }
        }

        private bool sendIfRightDoubleClick;
        public bool SendIfRightDoubleClick
        {
            get => sendIfRightDoubleClick;
            set
            {
                sendIfRightDoubleClick = value;
                OnPropertyChanged();
            }
        }

        private bool earthquakeNotification;
        public bool EarthquakeNotification
        {
            get => earthquakeNotification;
            set
            {
                earthquakeNotification = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> EarthquakeScales =>
            new ObservableCollection<string>(earthquakeScaleTable.Select(e => $"震度 {e[1]} 以上"));

        private string earthquakeMinScale;
        public string EarthquakeMinScale
        {
            get => earthquakeMinScale;
            set
            {
                earthquakeMinScale = value;
                OnPropertyChanged();
            }
        }

        private bool earthquakeShow;
        public bool EarthquakeShow
        {
            get => earthquakeShow;
            set
            {
                earthquakeShow = value;
                OnPropertyChanged();
            }
        }

        private bool earthquakeNotice;
        public bool EarthquakeNotice
        {
            get => earthquakeNotice;
            set
            {
                earthquakeNotice = value;
                OnPropertyChanged();
            }
        }

        private bool earthquakeSound;
        public bool EarthquakeSound
        {
            get => earthquakeSound;
            set
            {
                earthquakeSound = value;
                OnPropertyChanged();
            }
        }

        private bool userquakeNotification;
        public bool UserquakeNotification
        {
            get => userquakeNotification;
            set
            {
                userquakeNotification = value;
                OnPropertyChanged();
            }
        }

        private bool userquakeShow;
        public bool UserquakeShow
        {
            get => userquakeShow;
            set
            {
                userquakeShow = value;
                OnPropertyChanged();
            }
        }

        private bool userquakeNotice;
        public bool UserquakeNotice
        {
            get => userquakeNotice;
            set
            {
                userquakeNotice = value;
                OnPropertyChanged();
            }
        }

        private bool userquakeSound;
        public bool UserquakeSound
        {
            get => userquakeSound;
            set
            {
                userquakeSound = value;
                OnPropertyChanged();
            }
        }

        private bool tsunamiNotification;
        public bool TsunamiNotification
        {
            get => tsunamiNotification;
            set
            {
                tsunamiNotification = value;
                OnPropertyChanged();
            }
        }

        private bool tsunamiShow;
        public bool TsunamiShow
        {
            get => tsunamiShow;
            set
            {
                tsunamiShow = value;
                OnPropertyChanged();
            }
        }

        private bool tsunamiNotice;
        public bool TsunamiNotice
        {
            get => tsunamiNotice;
            set
            {
                tsunamiNotice = value;
                OnPropertyChanged();
            }
        }

        private bool tsunamiSound;
        public bool TsunamiSound
        {
            get => tsunamiSound;
            set
            {
                tsunamiSound = value;
                OnPropertyChanged();
            }
        }

        private bool eewTestNotification;
        public bool EEWTestNotification
        {
            get => eewTestNotification;
            set
            {
                eewTestNotification = value;
                OnPropertyChanged();
            }
        }

        private bool eewTestShow;
        public bool EEWTestShow
        {
            get => eewTestShow;
            set
            {
                eewTestShow = value;
                OnPropertyChanged();
            }
        }

        private bool eewTestNotice;
        public bool EEWTestNotice
        {
            get => eewTestNotice;
            set
            {
                eewTestNotice = value;
                OnPropertyChanged();
            }
        }

        private bool eewTestSound;
        public bool EEWTestSound
        {
            get => eewTestSound;
            set
            {
                eewTestSound = value;
                OnPropertyChanged();
            }
        }

        private bool eewVoiceGuidance;
        public bool EEWVoiceGuidance
        {
            get => eewVoiceGuidance;
            set
            {
                eewVoiceGuidance = value;
                OnPropertyChanged();
            }
        }
            
        public void LoadFromConfiguration(Configuration configuration)
        {
            bootAtStartup = configuration.BootAtStartup;
            minimizeAtBoot = configuration.MinimizeAtBoot;
            autoUpdate = configuration.AutoUpdate;

            portOpen = configuration.PortOpen;
            useUPnP = configuration.UseUPnP;
            port = configuration.Port;
            disconnectionComplement = configuration.DisconnectionComplement;

            selectArea = ConvertAreaCodeToLabel(configuration.AreaCode);
            sendIfMiddleDoubleClick = configuration.SendIfMiddleDoubleClick;
            sendIfRightDoubleClick = configuration.SendIfRightDoubleClick;

            // XXX: この羅列… なんとかならんのか？
            earthquakeNotification = configuration.EarthquakeNotification.Enabled;
            earthquakeMinScale = ConvertScaleValueToLabel(configuration.EarthquakeNotification.MinScale);
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
            configuration.AutoUpdate = autoUpdate;

            configuration.PortOpen = portOpen;
            configuration.UseUPnP = useUPnP;
            configuration.Port = port;
            configuration.DisconnectionComplement = disconnectionComplement;

            configuration.AreaCode = ConvertAreaToAreaCode(selectArea);
            configuration.SendIfMiddleDoubleClick = sendIfMiddleDoubleClick;
            configuration.SendIfRightDoubleClick = sendIfRightDoubleClick;

            configuration.EarthquakeNotification.Enabled = earthquakeNotification;
            configuration.EarthquakeNotification.MinScale = ConvertScaleLabelToValue(earthquakeMinScale);
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

        private int ConvertScaleLabelToValue(string scaleLabel)
        {
            var simplifyScaleLabel = scaleLabel.Replace("震度", "").Replace("以上", "").Replace(" ", "");
            return int.Parse(earthquakeScaleTable.First(e => e[1] == simplifyScaleLabel)[0]);
        }

        private string ConvertScaleValueToLabel(int scale)
        {
            var simplifyScaleLabel = earthquakeScaleTable.First(e => int.Parse(e[0]) == scale)[1];
            return $"震度 {simplifyScaleLabel} 以上";
        }

        private int ConvertAreaToAreaCode(string areaLabel)
        {
            var item = areaCodeLabels.FirstOrDefault(e => e.label == areaLabel);
            return item == default ? 900 : item.code;
        }

        private string ConvertAreaCodeToLabel(int areaCode)
        {
            var item = areaCodeLabels.FirstOrDefault(e => e.code == areaCode);
            return item == default ? "未設定" : item.label;
        }

        // See: https://docs.microsoft.com/ja-jp/dotnet/desktop/wpf/data/how-to-implement-property-change-notification
        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            WritebackConfiguration();
        }
    }
}
