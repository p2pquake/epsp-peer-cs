using MP3Sharp;

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace AvaloniaClient.Notifications;

/// <summary>
/// クロスプラットフォーム MP3 再生。
/// MP3Sharp (pure C# MP3 decoder) + プラットフォーム API (P/Invoke) を使用。
/// </summary>
internal static class AudioPlayer
{
    public static void PlayMp3(string filePath)
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                PlayMp3Windows(filePath);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                PlayMp3Linux(filePath);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // afplay は macOS 標準コマンド
                Process.Start("afplay", filePath)?.WaitForExit();
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[AudioPlayer] Playback error: {ex.Message}");
        }
    }

    // =========================================================
    // MP3Sharp デコード (常にステレオ出力 → モノラルに変換)
    // =========================================================
    private static (byte[] pcm, int sampleRate) DecodeMp3(string filePath)
    {
        using var mp3 = new MP3Stream(filePath);
        using var stereoStream = new MemoryStream();
        var buffer = new byte[4096];
        int bytesRead;
        while ((bytesRead = mp3.Read(buffer, 0, buffer.Length)) > 0)
        {
            stereoStream.Write(buffer, 0, bytesRead);
        }

        var stereo = stereoStream.ToArray();

        // MP3Sharp は常に 16-bit ステレオで出力するが、
        // モノラル音源の場合は左チャンネルのみにデータが入る。
        // 左チャンネルを抽出してモノラルにする。
        var mono = new byte[stereo.Length / 2];
        for (int i = 0, j = 0; i < stereo.Length; i += 4, j += 2)
        {
            mono[j] = stereo[i];
            mono[j + 1] = stereo[i + 1];
        }

        return (mono, mp3.Frequency);
    }

    // =========================================================
    // Windows: PCM WAV → winmm PlaySound
    // =========================================================
    private static void PlayMp3Windows(string filePath)
    {
        var (pcm, sampleRate) = DecodeMp3(filePath);
        var wav = CreateWav(pcm, sampleRate, 1, 16);
        PlaySound(wav, IntPtr.Zero, SND_MEMORY | SND_SYNC | SND_NODEFAULT);
    }

    // =========================================================
    // Linux: PCM → PulseAudio Simple API
    // =========================================================
    private static void PlayMp3Linux(string filePath)
    {
        var (pcm, sampleRate) = DecodeMp3(filePath);

        var ss = new pa_sample_spec
        {
            format = PA_SAMPLE_S16LE,
            rate = (uint)sampleRate,
            channels = 1,
        };

        var pa = pa_simple_new(
            IntPtr.Zero, "P2PQuake", PA_STREAM_PLAYBACK,
            IntPtr.Zero, "notification",
            ref ss, IntPtr.Zero, IntPtr.Zero, out int error);

        if (pa == IntPtr.Zero)
        {
            Console.Error.WriteLine($"[AudioPlayer] PulseAudio init failed (error={error})");
            return;
        }

        try
        {
            pa_simple_write(pa, pcm, (UIntPtr)pcm.Length, out error);
            pa_simple_drain(pa, out error);
        }
        finally
        {
            pa_simple_free(pa);
        }
    }

    // =========================================================
    // Windows P/Invoke: winmm.dll
    // =========================================================
    [DllImport("winmm.dll", SetLastError = true)]
    private static extern bool PlaySound(byte[] pszSound, IntPtr hmod, uint fdwSound);

    private const uint SND_MEMORY = 0x0004;
    private const uint SND_SYNC = 0x0000;
    private const uint SND_NODEFAULT = 0x0002;

    // =========================================================
    // Linux P/Invoke: libpulse-simple.so.0 (PulseAudio Simple API)
    // =========================================================
    private const int PA_SAMPLE_S16LE = 3;
    private const int PA_STREAM_PLAYBACK = 1;

    [StructLayout(LayoutKind.Sequential)]
    private struct pa_sample_spec
    {
        public int format;
        public uint rate;
        public byte channels;
    }

    [DllImport("libpulse-simple.so.0")]
    private static extern IntPtr pa_simple_new(
        IntPtr server, string name, int dir, IntPtr dev,
        string stream_name, ref pa_sample_spec ss,
        IntPtr channel_map, IntPtr attr, out int error);

    [DllImport("libpulse-simple.so.0")]
    private static extern int pa_simple_write(IntPtr s, byte[] data, UIntPtr bytes, out int error);

    [DllImport("libpulse-simple.so.0")]
    private static extern int pa_simple_drain(IntPtr s, out int error);

    [DllImport("libpulse-simple.so.0")]
    private static extern void pa_simple_free(IntPtr s);

    // =========================================================
    // WAV ヘッダ生成 (Windows PlaySound 用)
    // =========================================================
    private static byte[] CreateWav(byte[] pcmData, int sampleRate, int channels, int bitsPerSample)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        int byteRate = sampleRate * channels * bitsPerSample / 8;
        int blockAlign = channels * bitsPerSample / 8;

        bw.Write(Encoding.ASCII.GetBytes("RIFF"));
        bw.Write(36 + pcmData.Length);
        bw.Write(Encoding.ASCII.GetBytes("WAVE"));
        bw.Write(Encoding.ASCII.GetBytes("fmt "));
        bw.Write(16);
        bw.Write((short)1); // PCM
        bw.Write((short)channels);
        bw.Write(sampleRate);
        bw.Write(byteRate);
        bw.Write((short)blockAlign);
        bw.Write((short)bitsPerSample);
        bw.Write(Encoding.ASCII.GetBytes("data"));
        bw.Write(pcmData.Length);
        bw.Write(pcmData);

        return ms.ToArray();
    }
}
