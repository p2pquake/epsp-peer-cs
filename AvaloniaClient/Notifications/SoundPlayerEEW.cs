using Client.Peer;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AvaloniaClient.Notifications;

internal class SoundPlayerEEW
{
    // 重複再生しないための手抜き制御
    private static string playingUUID = "";

    public static void PlayEEWAsync(EPSPEEWEventArgs eew)
    {
        playingUUID = Guid.NewGuid().ToString();

        Task.Run(() =>
        {
            PlayEEW(eew, playingUUID);
        });
    }

    private static void PlayEEW(EPSPEEWEventArgs eew, string uuid)
    {
        var playIds = new List<string>();
        if (eew.IsCancelled)
        {
            playIds.Add("eew_cancelled");
        }
        else if (eew.Areas == null || eew.Areas.Length == 0)
        {
            playIds.Add(eew.IsFollowUp ? "eew_followup" : "eew");
            playIds.Add("guidance");
        }
        else
        {
            playIds.Add(eew.IsFollowUp ? "eew_followup" : "eew");
            playIds.Add("announce_areas");
            foreach (var area in eew.Areas)
            {
                playIds.Add($"{area}");
            }
            playIds.AddRange(playIds.ToArray());
            playIds.Add("guidance");
        }

        foreach (var playId in playIds)
        {
            try
            {
                if (uuid != playingUUID) { return; }
                PlaySound(playId);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"[SoundPlayerEEW] Error playing {playId}: {e.Message}");
            }
        }
    }

    private static void PlaySound(string playId)
    {
        var tempPath = Player.ExtractSoundToTemp(playId, "EEW");
        if (tempPath == null) return;

        AudioPlayer.PlayMp3(tempPath);
    }
}
