using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace WpfClient
{
    public class ConfigurationManager
    {
        private static readonly string fileName = "example.json";

        private static Configuration configuration;
        public static Configuration Configuration
        {
            get
            {
                if (configuration == null)
                {
                    configuration = Load();
                }

                return configuration;
            }
        }

        public static bool Save()
        {
            try
            {
                string json = JsonSerializer.Serialize(Configuration);
                File.WriteAllText(fileName, json);
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
            if (!File.Exists(fileName))
            {
                return new Configuration();
            }

            try
            {
                string json = File.ReadAllText(fileName);
                return JsonSerializer.Deserialize<Configuration>(json);
            }
            catch (Exception)
            {
                // FIXME: エラーハンドリングする
                return new Configuration();
            }
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
        // 起動
        public bool BootAtStartup { get; set; } = false;
        public bool MinimizeAtBoot { get; set; } = false;

        // 接続
        public bool PortOpen { get; set; } = true;
        public bool UseUPnP { get; set; } = true;
        public int Port { get; set; } = 6911;

        // 「揺れた！」
        public string Area { get; set; } = "901";
        public bool SendIfMiddleDoubleClick { get; set; } = false;
        public bool SendIfRightDoubleClick { get; set; } = false;

        // 表示・通知
        public EarthquakeNotification EarthquakeNotification { get; set; } = new EarthquakeNotification();
        public Notification UserquakeNotification { get; set; } = new Notification();
        public Notification TsunamiNotification { get; set; } = new Notification();
        public Notification EEWTestNotification { get; set; } = new Notification();
    }
}
