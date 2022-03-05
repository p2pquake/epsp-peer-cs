using Microsoft.Win32;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WpfClient
{
    public class ConfigurationManager
    {
        private static Configuration configuration;
        public static Configuration Configuration
        {
            get
            {
                if (configuration == null)
                {
                    configuration = Load();
                    Configuration.ReflectBootAtStartup(configuration.BootAtStartup);
                }

                return configuration;
            }
        }

        public static bool Save()
        {
            try
            {
                string json = JsonSerializer.Serialize(Configuration);
                File.WriteAllText(Filename(), json);
                return true;
            }
            catch (Exception)
            {
                // FIXME: エラーハンドリングする
                return false;
            }
        }

        private static Configuration Load()
        {
            if (!File.Exists(Filename()))
            {
                return new Configuration();
            }

            try
            {
                string json = File.ReadAllText(Filename());
                return JsonSerializer.Deserialize<Configuration>(json);
            }
            catch (Exception)
            {
                // FIXME: エラーハンドリングする
                return new Configuration();
            }
        }

        private static string Filename()
        {
            return Path.ChangeExtension(Application.ExecutablePath, ".json");
        }
    }

    public class Notification
    {
        public bool Enabled { get; set; }
        public bool Show { get; set; }
        public bool Notice { get; set; }
        public bool Sound { get; set; }
    }

    public class EarthquakeNotification : Notification
    {
        public int MinScale { get; set; } = 10;
    }

    public class Configuration
    {
        public event EventHandler OnChangeEPSPConfiguration = (s, e) => { };

        // 起動
        private bool bootAtStartup = false;
        public bool BootAtStartup
        {
            get => bootAtStartup;
            set
            {
                bootAtStartup = value;
                ReflectBootAtStartup(value);
            }
        }

        public bool MinimizeAtBoot { get; set; } = false;
        public bool AutoUpdate { get; set; } = true;

        // 接続
        private bool portOpen = true;
        public bool PortOpen
        {
            get => portOpen;
            set
            {
                portOpen = value;
                OnChangeEPSPConfiguration(this, EventArgs.Empty);
            }
        }

        private bool useUPnP = true;
        public bool UseUPnP
        {
            get => useUPnP;
            set {
                useUPnP = value;
                OnChangeEPSPConfiguration(this, EventArgs.Empty);
            }
        }

        private int port = 6911;
        public int Port
        {
            get => port;
            set
            {
                port = value;
                OnChangeEPSPConfiguration(this, EventArgs.Empty);
            }
        }

        private bool disconnectionComplement = true;
        public bool DisconnectionComplement
        {
            get => disconnectionComplement;
            set
            {
                disconnectionComplement = value;
                OnChangeEPSPConfiguration(this, EventArgs.Empty);
            }
        }

        // 「揺れた！」
        private int areaCode = 900;
        public int AreaCode
        {
            get => areaCode;
            set
            {
                areaCode = value;
                OnChangeEPSPConfiguration(this, EventArgs.Empty);
            }
        }

        public bool SendIfMiddleDoubleClick { get; set; } = false;
        public bool SendIfRightDoubleClick { get; set; } = false;

        // 表示・通知
        public EarthquakeNotification EarthquakeNotification { get; set; } = new EarthquakeNotification();
        public Notification UserquakeNotification { get; set; } = new Notification();
        public Notification TsunamiNotification { get; set; } = new Notification();
        public Notification EEWTestNotification { get; set; } = new Notification();

        // XXX: このメソッドはここにあるべきなのか？
        public static void ReflectBootAtStartup(bool bootAtStartup)
        {
            var regKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
            if (bootAtStartup)
            {
                regKey.SetValue(Application.ProductName, Application.ExecutablePath);
                return;
            }
            if (regKey.GetValue(Application.ProductName) != null)
            {
                regKey.DeleteValue(Application.ProductName);
            }
        }
    }
}
