using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;

namespace AvaloniaUIClient.ViewModels
{
    public partial class ConfigurationViewModel : ViewModelBase
    {
        private Configuration _configuration;

        [ObservableProperty]
        private string _selectedCategory = "起動";

        public ObservableCollection<string> Categories { get; } = new()
        {
            "起動",
            "接続", 
            "「揺れた！」",
            "表示・通知"
        };

        public ObservableCollection<AreaItem> AreaList { get; } = new();

        public ConfigurationViewModel()
        {
            _configuration = ConfigurationManager.LoadConfiguration();
            LoadAreaList();
            
            // プロパティ変更監視を設定
            PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // 設定値が変更されたら自動保存
            if (e.PropertyName != nameof(SelectedCategory))
            {
                ConfigurationManager.SaveConfiguration(_configuration);
            }
            
            // カテゴリ変更時に可視性プロパティを更新
            if (e.PropertyName == nameof(SelectedCategory))
            {
                OnPropertyChanged(nameof(IsBootupCategoryVisible));
                OnPropertyChanged(nameof(IsConnectionCategoryVisible));
                OnPropertyChanged(nameof(IsUserquakeCategoryVisible));
                OnPropertyChanged(nameof(IsNotificationCategoryVisible));
            }
        }

        public bool IsBootupCategoryVisible => SelectedCategory == "起動";
        public bool IsConnectionCategoryVisible => SelectedCategory == "接続";
        public bool IsUserquakeCategoryVisible => SelectedCategory == "「揺れた！」";
        public bool IsNotificationCategoryVisible => SelectedCategory == "表示・通知";

        private void LoadAreaList()
        {
            try
            {
                // 地域データをロード（WPFClientの epsp-area.csv を参考に実装）
                AreaList.Clear();
                AreaList.Add(new AreaItem { Code = 900, Name = "地域未設定" });
                
                // 実際の地域データは後で実装
                // 現在はプレースホルダーとして基本的な地域のみ追加
                var basicAreas = new[]
                {
                    new { Code = 100, Name = "石狩地方北部" },
                    new { Code = 101, Name = "石狩地方中部" },
                    new { Code = 102, Name = "石狩地方南部" },
                    new { Code = 230, Name = "東京都23区" },
                    new { Code = 231, Name = "東京都多摩東部" },
                    new { Code = 232, Name = "東京都多摩西部" },
                    new { Code = 340, Name = "横浜・川崎" },
                    new { Code = 341, Name = "湘南" },
                    new { Code = 571, Name = "大阪府北部" },
                    new { Code = 572, Name = "大阪府南部" }
                };

                foreach (var area in basicAreas)
                {
                    AreaList.Add(new AreaItem { Code = area.Code, Name = area.Name });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"地域データ読み込みエラー: {ex.Message}");
            }
        }

        // 起動設定
        public bool BootAtStartup
        {
            get => _configuration.BootAtStartup;
            set
            {
                if (_configuration.BootAtStartup != value)
                {
                    _configuration.BootAtStartup = value;
                    UpdateWindowsStartup(value);
                    OnPropertyChanged();
                }
            }
        }

        public bool MinimizeAtBoot
        {
            get => _configuration.MinimizeAtBoot;
            set
            {
                if (_configuration.MinimizeAtBoot != value)
                {
                    _configuration.MinimizeAtBoot = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool AutoUpdate
        {
            get => _configuration.AutoUpdate;
            set
            {
                if (_configuration.AutoUpdate != value)
                {
                    _configuration.AutoUpdate = value;
                    OnPropertyChanged();
                }
            }
        }

        // 接続設定
        public bool PortOpen
        {
            get => _configuration.PortOpen;
            set
            {
                if (_configuration.PortOpen != value)
                {
                    _configuration.PortOpen = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsUPnPEnabled));
                }
            }
        }

        public bool UseUPnP
        {
            get => _configuration.UseUPnP;
            set
            {
                if (_configuration.UseUPnP != value)
                {
                    _configuration.UseUPnP = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsUPnPEnabled => PortOpen;

        public int Port
        {
            get => _configuration.Port;
            set
            {
                if (_configuration.Port != value && value > 0 && value <= 65535)
                {
                    _configuration.Port = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool DisconnectionComplement
        {
            get => _configuration.DisconnectionComplement;
            set
            {
                if (_configuration.DisconnectionComplement != value)
                {
                    _configuration.DisconnectionComplement = value;
                    OnPropertyChanged();
                }
            }
        }

        // 「揺れた！」設定
        public int AreaCode
        {
            get => _configuration.AreaCode;
            set
            {
                if (_configuration.AreaCode != value)
                {
                    _configuration.AreaCode = value;
                    OnPropertyChanged();
                }
            }
        }

        public AreaItem? SelectedArea
        {
            get => AreaList.FirstOrDefault(a => a.Code == AreaCode);
            set
            {
                if (value != null && AreaCode != value.Code)
                {
                    AreaCode = value.Code;
                    OnPropertyChanged();
                }
            }
        }

        public bool SendIfMiddleDoubleClick
        {
            get => _configuration.SendIfMiddleDoubleClick;
            set
            {
                if (_configuration.SendIfMiddleDoubleClick != value)
                {
                    _configuration.SendIfMiddleDoubleClick = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool SendIfRightDoubleClick
        {
            get => _configuration.SendIfRightDoubleClick;
            set
            {
                if (_configuration.SendIfRightDoubleClick != value)
                {
                    _configuration.SendIfRightDoubleClick = value;
                    OnPropertyChanged();
                }
            }
        }

        // 地震情報通知設定
        public bool EarthquakeEnabled
        {
            get => _configuration.EarthquakeNotification.Enabled;
            set
            {
                if (_configuration.EarthquakeNotification.Enabled != value)
                {
                    _configuration.EarthquakeNotification.Enabled = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsEarthquakeSettingsEnabled));
                }
            }
        }

        public bool IsEarthquakeSettingsEnabled => EarthquakeEnabled;

        public int EarthquakeMinScale
        {
            get => _configuration.EarthquakeNotification.MinScale;
            set
            {
                if (_configuration.EarthquakeNotification.MinScale != value)
                {
                    _configuration.EarthquakeNotification.MinScale = value;
                    OnPropertyChanged();
                }
            }
        }

        public string EarthquakeMinScaleText
        {
            get => ConvertScaleToString(EarthquakeMinScale);
            set
            {
                var scale = ConvertStringToScale(value);
                if (scale != EarthquakeMinScale)
                {
                    EarthquakeMinScale = scale;
                    OnPropertyChanged();
                }
            }
        }

        public bool EarthquakeForeign
        {
            get => _configuration.EarthquakeNotification.Foreign;
            set
            {
                if (_configuration.EarthquakeNotification.Foreign != value)
                {
                    _configuration.EarthquakeNotification.Foreign = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool EarthquakeShow
        {
            get => _configuration.EarthquakeNotification.Show;
            set
            {
                if (_configuration.EarthquakeNotification.Show != value)
                {
                    _configuration.EarthquakeNotification.Show = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool EarthquakeNotice
        {
            get => _configuration.EarthquakeNotification.Notice;
            set
            {
                if (_configuration.EarthquakeNotification.Notice != value)
                {
                    _configuration.EarthquakeNotification.Notice = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool EarthquakeSound
        {
            get => _configuration.EarthquakeNotification.Sound;
            set
            {
                if (_configuration.EarthquakeNotification.Sound != value)
                {
                    _configuration.EarthquakeNotification.Sound = value;
                    OnPropertyChanged();
                }
            }
        }

        // 地震感知情報通知設定
        public bool UserquakeEnabled
        {
            get => _configuration.UserquakeNotification.Enabled;
            set
            {
                if (_configuration.UserquakeNotification.Enabled != value)
                {
                    _configuration.UserquakeNotification.Enabled = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsUserquakeSettingsEnabled));
                }
            }
        }

        public bool IsUserquakeSettingsEnabled => UserquakeEnabled;

        public bool UserquakeShow
        {
            get => _configuration.UserquakeNotification.Show;
            set
            {
                if (_configuration.UserquakeNotification.Show != value)
                {
                    _configuration.UserquakeNotification.Show = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool UserquakeNotice
        {
            get => _configuration.UserquakeNotification.Notice;
            set
            {
                if (_configuration.UserquakeNotification.Notice != value)
                {
                    _configuration.UserquakeNotification.Notice = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool UserquakeSound
        {
            get => _configuration.UserquakeNotification.Sound;
            set
            {
                if (_configuration.UserquakeNotification.Sound != value)
                {
                    _configuration.UserquakeNotification.Sound = value;
                    OnPropertyChanged();
                }
            }
        }

        // 津波予報通知設定
        public bool TsunamiEnabled
        {
            get => _configuration.TsunamiNotification.Enabled;
            set
            {
                if (_configuration.TsunamiNotification.Enabled != value)
                {
                    _configuration.TsunamiNotification.Enabled = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsTsunamiSettingsEnabled));
                }
            }
        }

        public bool IsTsunamiSettingsEnabled => TsunamiEnabled;

        public bool TsunamiShow
        {
            get => _configuration.TsunamiNotification.Show;
            set
            {
                if (_configuration.TsunamiNotification.Show != value)
                {
                    _configuration.TsunamiNotification.Show = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool TsunamiNotice
        {
            get => _configuration.TsunamiNotification.Notice;
            set
            {
                if (_configuration.TsunamiNotification.Notice != value)
                {
                    _configuration.TsunamiNotification.Notice = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool TsunamiSound
        {
            get => _configuration.TsunamiNotification.Sound;
            set
            {
                if (_configuration.TsunamiNotification.Sound != value)
                {
                    _configuration.TsunamiNotification.Sound = value;
                    OnPropertyChanged();
                }
            }
        }

        // 緊急地震速報通知設定
        public bool EEWEnabled
        {
            get => _configuration.EEWTestNotification.Enabled;
            set
            {
                if (_configuration.EEWTestNotification.Enabled != value)
                {
                    _configuration.EEWTestNotification.Enabled = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsEEWSettingsEnabled));
                }
            }
        }

        public bool IsEEWSettingsEnabled => EEWEnabled;

        public bool EEWShow
        {
            get => _configuration.EEWTestNotification.Show;
            set
            {
                if (_configuration.EEWTestNotification.Show != value)
                {
                    _configuration.EEWTestNotification.Show = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool EEWNotice
        {
            get => _configuration.EEWTestNotification.Notice;
            set
            {
                if (_configuration.EEWTestNotification.Notice != value)
                {
                    _configuration.EEWTestNotification.Notice = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool EEWSound
        {
            get => _configuration.EEWTestNotification.Sound;
            set
            {
                if (_configuration.EEWTestNotification.Sound != value)
                {
                    _configuration.EEWTestNotification.Sound = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool EEWVoiceGuidance
        {
            get => _configuration.EEWTestNotification.VoiceGuidance;
            set
            {
                if (_configuration.EEWTestNotification.VoiceGuidance != value)
                {
                    _configuration.EEWTestNotification.VoiceGuidance = value;
                    OnPropertyChanged();
                }
            }
        }

        private void UpdateWindowsStartup(bool enable)
        {
            try
            {
                var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                if (key != null)
                {
                    var appName = "P2PQuake";
                    if (enable)
                    {
                        var exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                        if (exePath.EndsWith(".dll"))
                        {
                            exePath = exePath.Replace(".dll", ".exe");
                        }
                        key.SetValue(appName, $"\"{exePath}\"");
                    }
                    else
                    {
                        key.DeleteValue(appName, false);
                    }
                    key.Close();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"レジストリ更新エラー: {ex.Message}");
            }
        }

        private static string ConvertScaleToString(int scale)
        {
            return scale switch
            {
                10 => "1",
                20 => "2", 
                30 => "3",
                40 => "4",
                45 => "5弱",
                50 => "5強",
                55 => "6弱",
                60 => "6強",
                70 => "7",
                _ => "1"
            };
        }

        private static int ConvertStringToScale(string scaleText)
        {
            return scaleText switch
            {
                "1" => 10,
                "2" => 20,
                "3" => 30,
                "4" => 40,
                "5弱" => 45,
                "5強" => 50,
                "6弱" => 55,
                "6強" => 60,
                "7" => 70,
                _ => 10
            };
        }
    }

    public class AreaItem
    {
        public int Code { get; set; }
        public string Name { get; set; } = string.Empty;

        public override string ToString() => Name;
    }
}

// 設定データクラス群
public class Configuration
{
    // 起動設定
    public bool BootAtStartup { get; set; } = false;
    public bool MinimizeAtBoot { get; set; } = false;
    public bool AutoUpdate { get; set; } = true;

    // 接続設定
    public bool PortOpen { get; set; } = true;
    public bool UseUPnP { get; set; } = true;
    public int Port { get; set; } = 6911;
    public bool DisconnectionComplement { get; set; } = true;

    // 「揺れた！」設定
    public int AreaCode { get; set; } = 900;
    public bool SendIfMiddleDoubleClick { get; set; } = false;
    public bool SendIfRightDoubleClick { get; set; } = false;

    // 通知設定
    public EarthquakeNotification EarthquakeNotification { get; set; } = new();
    public Notification UserquakeNotification { get; set; } = new();
    public Notification TsunamiNotification { get; set; } = new();
    public Notification EEWTestNotification { get; set; } = new();
}

public class Notification
{
    public bool Enabled { get; set; } = true;
    public bool Show { get; set; } = true;
    public bool Notice { get; set; } = false;
    public bool Sound { get; set; } = true;
    public bool VoiceGuidance { get; set; } = false;
}

public class EarthquakeNotification : Notification
{
    public int MinScale { get; set; } = 10;
    public bool Foreign { get; set; } = false;
}

public static class ConfigurationManager
{
    private static Configuration? _configuration;
    private static readonly string ConfigFilePath = GetConfigFilePath();

    private static string GetConfigFilePath()
    {
        var exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        var exeDir = Path.GetDirectoryName(exePath) ?? "";
        var exeName = Path.GetFileNameWithoutExtension(exePath);
        return Path.Combine(exeDir, $"{exeName}.json");
    }

    public static Configuration LoadConfiguration()
    {
        if (_configuration != null)
            return _configuration;

        try
        {
            if (File.Exists(ConfigFilePath))
            {
                var json = File.ReadAllText(ConfigFilePath);
                _configuration = JsonSerializer.Deserialize<Configuration>(json) ?? new Configuration();
            }
            else
            {
                _configuration = new Configuration();
                SaveConfiguration(_configuration);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"設定読み込みエラー: {ex.Message}");
            _configuration = new Configuration();
        }

        return _configuration;
    }

    public static void SaveConfiguration(Configuration configuration)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            var json = JsonSerializer.Serialize(configuration, options);
            File.WriteAllText(ConfigFilePath, json);
            _configuration = configuration;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"設定保存エラー: {ex.Message}");
        }
    }
}