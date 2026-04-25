using MP3Sharp;

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

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
    // Windows: PCM → waveOut API (複数インスタンスで同時再生可能)
    // =========================================================
    private static void PlayMp3Windows(string filePath)
    {
        var (pcm, sampleRate) = DecodeMp3(filePath);

        var fmt = new WAVEFORMATEX
        {
            wFormatTag = WAVE_FORMAT_PCM,
            nChannels = 1,
            nSamplesPerSec = (uint)sampleRate,
            nAvgBytesPerSec = (uint)(sampleRate * 2),
            nBlockAlign = 2,
            wBitsPerSample = 16,
            cbSize = 0,
        };

        using var doneEvent = new ManualResetEvent(false);

        int mmr = waveOutOpen(out IntPtr hWaveOut, WAVE_MAPPER, ref fmt,
            doneEvent.SafeWaitHandle.DangerousGetHandle(), IntPtr.Zero, CALLBACK_EVENT);
        if (mmr != MMSYSERR_NOERROR)
        {
            Console.Error.WriteLine($"[AudioPlayer] waveOutOpen failed: {mmr}");
            return;
        }

        var gcPcm = GCHandle.Alloc(pcm, GCHandleType.Pinned);
        IntPtr pHdr = IntPtr.Zero;
        try
        {
            var hdr = new WAVEHDR
            {
                lpData = gcPcm.AddrOfPinnedObject(),
                dwBufferLength = (uint)pcm.Length,
                dwFlags = 0,
                dwLoops = 0,
            };
            uint hdrSize = (uint)Marshal.SizeOf<WAVEHDR>();
            pHdr = Marshal.AllocHGlobal((int)hdrSize);
            Marshal.StructureToPtr(hdr, pHdr, false);

            waveOutPrepareHeader(hWaveOut, pHdr, hdrSize);

            // WOM_OPEN で既にシグナル済みの場合に備えてリセット
            doneEvent.Reset();
            waveOutWrite(hWaveOut, pHdr, hdrSize);

            // WOM_DONE を待つ (最大 30 秒)
            doneEvent.WaitOne(30_000);

            waveOutUnprepareHeader(hWaveOut, pHdr, hdrSize);
        }
        finally
        {
            if (pHdr != IntPtr.Zero)
                Marshal.FreeHGlobal(pHdr);
            gcPcm.Free();
            waveOutClose(hWaveOut);
        }
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
    // Windows P/Invoke: winmm.dll waveOut API
    // =========================================================
    private const uint WAVE_MAPPER = unchecked((uint)-1);
    private const int MMSYSERR_NOERROR = 0;
    private const uint CALLBACK_EVENT = 0x00050000;
    private const ushort WAVE_FORMAT_PCM = 1;

    [StructLayout(LayoutKind.Sequential)]
    private struct WAVEFORMATEX
    {
        public ushort wFormatTag;
        public ushort nChannels;
        public uint nSamplesPerSec;
        public uint nAvgBytesPerSec;
        public ushort nBlockAlign;
        public ushort wBitsPerSample;
        public ushort cbSize;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct WAVEHDR
    {
        public IntPtr lpData;
        public uint dwBufferLength;
        public uint dwBytesRecorded;
        public IntPtr dwUser;
        public uint dwFlags;
        public uint dwLoops;
        public IntPtr lpNext;
        public IntPtr reserved;
    }

    [DllImport("winmm.dll")]
    private static extern int waveOutOpen(out IntPtr hWaveOut, uint uDeviceID,
        ref WAVEFORMATEX lpFormat, IntPtr dwCallback, IntPtr dwInstance, uint fdwOpen);

    [DllImport("winmm.dll")]
    private static extern int waveOutPrepareHeader(IntPtr hWaveOut, IntPtr lpWaveOutHdr, uint uSize);

    [DllImport("winmm.dll")]
    private static extern int waveOutWrite(IntPtr hWaveOut, IntPtr lpWaveOutHdr, uint uSize);

    [DllImport("winmm.dll")]
    private static extern int waveOutUnprepareHeader(IntPtr hWaveOut, IntPtr lpWaveOutHdr, uint uSize);

    [DllImport("winmm.dll")]
    private static extern int waveOutClose(IntPtr hWaveOut);

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
}
