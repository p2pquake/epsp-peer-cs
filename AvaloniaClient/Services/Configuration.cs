using System;
using System.IO;
using System.Text.Json;

namespace AvaloniaClient.Services;

public class ConfigurationManager
{
    public static bool IsFirstBoot { get; set; } = false;

    private static Configuration? configuration;
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
            File.WriteAllText(Filename(), json);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static Configuration Load()
    {
        if (!File.Exists(Filename()))
        {
            IsFirstBoot = true;
            return new Configuration();
        }

        try
        {
            string json = File.ReadAllText(Filename());
            return JsonSerializer.Deserialize<Configuration>(json) ?? new Configuration();
        }
        catch (Exception)
        {
            return new Configuration();
        }
    }

    private static string Filename()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var dir = Path.Combine(appData, "P2PQuake");
        Directory.CreateDirectory(dir);
        return Path.Combine(dir, "config.json");
    }
}

public class Notification
{
    public bool Enabled { get; set; }
    public bool Show { get; set; }
    public bool Notice { get; set; }
    public bool Sound { get; set; }
    public bool VoiceGuidance { get; set; }
}

public class EarthquakeNotification : Notification
{
    public int MinScale { get; set; } = 10;
    public bool Foreign { get; set; }
}

public class Configuration
{
    public event EventHandler OnChangeEPSPConfiguration = (s, e) => { };

    // Boot
    public bool BootAtStartup { get; set; } = false;
    public bool MinimizeAtBoot { get; set; } = false;
    // Connection
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
        set
        {
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

    // Userquake
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

    // Notifications
    public EarthquakeNotification EarthquakeNotification { get; set; } = new() { Enabled = true, Notice = false, Show = true, Sound = true, Foreign = false };
    public Notification UserquakeNotification { get; set; } = new() { Enabled = true, Notice = false, Show = true, Sound = true };
    public Notification TsunamiNotification { get; set; } = new() { Enabled = true, Notice = false, Show = true, Sound = true };
    public Notification EEWTestNotification { get; set; } = new() { Enabled = true, Notice = false, Show = true, Sound = true, VoiceGuidance = true };
}
