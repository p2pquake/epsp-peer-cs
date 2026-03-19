using Client.App;
using Client.Peer;

using AvaloniaClient.Services;
using AvaloniaClient.Utils;

using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace AvaloniaClient.Notifications;

enum SoundType
{
    P2PQ_Snd0,
    P2PQ_Snd1,
    P2PQ_Snd2,
    P2PQ_Snd3,
    P2PQ_Snd4,
    P2PQ_Snd9,
    P2PQ_Sndt,
    EEW_Beta,
}

public class Player
{
    private Configuration configuration;
    private string lastEarthquakeOccuredTime = "";
    private SoundType lastSoundType = SoundType.P2PQ_Sndt;

    public Player(Configuration configuration, MediatorContext mediatorContext)
    {
        this.configuration = configuration;

        mediatorContext.OnEarthquake += (s, e) => { Task.Run(() => MediatorContext_OnEarthquake(s, e)); };
        mediatorContext.OnTsunami += (s, e) => { Task.Run(() => MediatorContext_OnTsunami(s, e)); };
        mediatorContext.OnEEW += (s, e) => { Task.Run(() => MediatorContext_OnEEW(s, e)); };
        mediatorContext.OnNewUserquakeEvaluation += (s, e) => { Task.Run(() => MediatorContext_OnNewUserquakeEvaluation(s, e)); };
    }

    private static async Task PlaySoundAsync(SoundType soundType)
    {
        await Task.Run(() => PlaySound(soundType));
    }

    private static void PlaySound(SoundType soundType)
    {
        try
        {
            var tempPath = ExtractSoundToTemp(soundType.ToString());
            if (tempPath == null) return;

            AudioPlayer.PlayMp3(tempPath);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[SoundPlayer] Error playing {soundType}: {ex.Message}");
        }
    }

    internal static string? ExtractSoundToTemp(string name, string subDir = "")
    {
        var assembly = Assembly.GetExecutingAssembly();
        var prefix = string.IsNullOrEmpty(subDir)
            ? "AvaloniaClient.Resources.Sounds."
            : $"AvaloniaClient.Resources.Sounds.{subDir}.";
        var resourceName = $"{prefix}{name}.mp3";

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            Console.Error.WriteLine($"[SoundPlayer] Resource not found: {resourceName}");
            return null;
        }

        var tempDir = Path.Combine(Path.GetTempPath(), "p2pquake_sounds");
        Directory.CreateDirectory(tempDir);
        var tempPath = Path.Combine(tempDir, $"{name}.mp3");

        if (!File.Exists(tempPath))
        {
            using var fs = new FileStream(tempPath, FileMode.Create, FileAccess.Write);
            stream.CopyTo(fs);
        }

        return tempPath;
    }

    private void MediatorContext_OnEarthquake(object? sender, EPSPQuakeEventArgs e)
    {
        if (e.InformationType == QuakeInformationType.Unknown)
        {
            return;
        }

        var earthquakeNotification = configuration.EarthquakeNotification;
        if (!earthquakeNotification.Enabled)
        {
            return;
        }
        if (!earthquakeNotification.Sound)
        {
            return;
        }

        // 震源情報を鳴動する必要性は低い
        if (e.InformationType == QuakeInformationType.Destination)
        {
            return;
        }

        var scale = ScaleConverter.Str2Int(e.Scale);
        var foreign = e.InformationType == QuakeInformationType.Foreign && earthquakeNotification.Foreign;
        if (scale < earthquakeNotification.MinScale && !foreign)
        {
            return;
        }

        var soundType = CalcSoundType(e.InformationType, scale);

        if (e.OccuredTime != lastEarthquakeOccuredTime)
        {
            _ = PlaySoundAsync(soundType);
        }
        else if (soundType != lastSoundType)
        {
            _ = PlaySoundAsync(soundType);
        }

        lastEarthquakeOccuredTime = e.OccuredTime;
        lastSoundType = soundType;
    }

    private SoundType CalcSoundType(QuakeInformationType informationType, int scale)
    {
        if (informationType == QuakeInformationType.Destination) { return SoundType.P2PQ_Snd0; }
        if (informationType == QuakeInformationType.Foreign) { return SoundType.P2PQ_Snd0; }
        if (scale >= 55) { return SoundType.P2PQ_Snd4; }
        if (scale >= 45) { return SoundType.P2PQ_Snd3; }
        if (scale >= 30) { return SoundType.P2PQ_Snd2; }
        return SoundType.P2PQ_Snd1;
    }

    private void MediatorContext_OnTsunami(object? sender, EPSPTsunamiEventArgs e)
    {
        var tsunamiNotification = configuration.TsunamiNotification;

        if (!tsunamiNotification.Enabled)
        {
            return;
        }
        if (!tsunamiNotification.Sound)
        {
            return;
        }

        _ = PlaySoundAsync(SoundType.P2PQ_Sndt);
    }

    private void MediatorContext_OnEEW(object? sender, EPSPEEWEventArgs e)
    {
        var eewTestNotification = configuration.EEWTestNotification;

        if (!eewTestNotification.Enabled)
        {
            return;
        }

        if (eewTestNotification.Sound && !e.IsCancelled)
        {
            _ = PlaySoundAsync(SoundType.EEW_Beta);
        }

        if (eewTestNotification.VoiceGuidance)
        {
            SoundPlayerEEW.PlayEEWAsync(e);
        }
    }

    private void MediatorContext_OnNewUserquakeEvaluation(object? sender, Client.App.Userquake.UserquakeEvaluateEventArgs e)
    {
        var userquakeNotification = configuration.UserquakeNotification;

        if (!userquakeNotification.Enabled)
        {
            return;
        }
        if (!userquakeNotification.Sound)
        {
            return;
        }

        _ = PlaySoundAsync(SoundType.P2PQ_Snd9);
    }
}
