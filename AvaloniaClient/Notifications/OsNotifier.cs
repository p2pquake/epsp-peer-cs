using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace AvaloniaClient.Notifications;

/// <summary>
/// クロスプラットフォーム OS ネイティブ通知。
/// Windows: Shell_NotifyIcon (P/Invoke) / Linux: notify-send / macOS: osascript.
/// </summary>
internal static class OsNotifier
{
    public static void Show(string title, string message)
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                ShowWindows(title, message);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                ShowLinux(title, message);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                ShowMacOS(title, message);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[OsNotifier] Show error: {ex.Message}");
        }
    }

    // =========================================================
    // Linux: notify-send (libnotify)
    // =========================================================
    private static void ShowLinux(string title, string message)
    {
        var psi = new ProcessStartInfo("notify-send")
        {
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        psi.ArgumentList.Add(title);
        psi.ArgumentList.Add(message);
        Process.Start(psi);
    }

    // =========================================================
    // macOS: osascript の display notification 構文
    // AppleScript はエスケープ規則が独特なので変換が必要
    // =========================================================
    private static void ShowMacOS(string title, string message)
    {
        var psi = new ProcessStartInfo("osascript")
        {
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        psi.ArgumentList.Add("-e");
        psi.ArgumentList.Add($"display notification \"{EscapeAppleScript(message)}\" with title \"{EscapeAppleScript(title)}\"");
        Process.Start(psi);
    }

    private static string EscapeAppleScript(string s)
    {
        // AppleScript リテラル文字列内で必要なエスケープ。
        // 改行は AppleScript の文字列リテラルでは扱いづらいのでスペースに変換する。
        return s
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\r", "")
            .Replace("\n", " ");
    }

    // =========================================================
    // Windows: Shell_NotifyIcon (P/Invoke)
    // 隠しメッセージウィンドウに NotifyIcon を登録 (NIS_HIDDEN) し、
    // NIM_MODIFY + NIF_INFO で balloon tip を表示する。
    // Windows 10+ では Action Center にトーストとして表示される。
    // =========================================================
    private static readonly object windowsLock = new();
    private static IntPtr hWnd = IntPtr.Zero;
    private static IntPtr hIcon = IntPtr.Zero;
    private static bool windowsInitialized = false;

    private static void ShowWindows(string title, string message)
    {
        lock (windowsLock)
        {
            if (!windowsInitialized)
            {
                InitWindows();
                windowsInitialized = true;
            }

            if (hWnd == IntPtr.Zero)
            {
                return;
            }

            var data = NewNotifyIconData();
            data.uFlags = NIF_INFO;
            data.szInfoTitle = Truncate(title, 63);
            data.szInfo = Truncate(message, 255);
            data.dwInfoFlags = NIIF_INFO;

            Shell_NotifyIconW(NIM_MODIFY, ref data);
        }
    }

    private static void InitWindows()
    {
        // メッセージ専用ウィンドウ (HWND_MESSAGE 配下) を作成。
        // STATIC は事前登録済みのウィンドウクラスなので RegisterClass 不要。
        hWnd = CreateWindowExW(0, "STATIC", "P2PQuakeOsNotifier", 0, 0, 0, 0, 0,
            HWND_MESSAGE, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
        if (hWnd == IntPtr.Zero)
        {
            Console.Error.WriteLine($"[OsNotifier] CreateWindowExW failed: {Marshal.GetLastPInvokeError()}");
            return;
        }

        // システム情報アイコン (NotifyIcon 登録には HICON が必要)
        hIcon = LoadIconW(IntPtr.Zero, new IntPtr(IDI_INFORMATION));

        var data = NewNotifyIconData();
        data.uFlags = NIF_ICON | NIF_TIP | NIF_STATE;
        data.hIcon = hIcon;
        data.szTip = "P2P地震情報";
        data.dwState = NIS_HIDDEN;
        data.dwStateMask = NIS_HIDDEN;

        if (!Shell_NotifyIconW(NIM_ADD, ref data))
        {
            Console.Error.WriteLine($"[OsNotifier] Shell_NotifyIcon NIM_ADD failed: {Marshal.GetLastPInvokeError()}");
            DestroyWindow(hWnd);
            hWnd = IntPtr.Zero;
            return;
        }

        AppDomain.CurrentDomain.ProcessExit += (_, _) => CleanupWindows();
    }

    private static void CleanupWindows()
    {
        lock (windowsLock)
        {
            if (hWnd == IntPtr.Zero)
            {
                return;
            }
            var data = NewNotifyIconData();
            Shell_NotifyIconW(NIM_DELETE, ref data);
            DestroyWindow(hWnd);
            hWnd = IntPtr.Zero;
        }
    }

    private static NOTIFYICONDATAW NewNotifyIconData() => new()
    {
        cbSize = (uint)Marshal.SizeOf<NOTIFYICONDATAW>(),
        hWnd = hWnd,
        uID = 1,
        szTip = string.Empty,
        szInfo = string.Empty,
        szInfoTitle = string.Empty,
    };

    private static string Truncate(string s, int maxChars) =>
        s.Length > maxChars ? s.Substring(0, maxChars) : s;

    // ----- Win32 P/Invoke -----
    private const uint NIM_ADD = 0x00000000;
    private const uint NIM_MODIFY = 0x00000001;
    private const uint NIM_DELETE = 0x00000002;

    private const uint NIF_ICON = 0x00000002;
    private const uint NIF_TIP = 0x00000004;
    private const uint NIF_STATE = 0x00000008;
    private const uint NIF_INFO = 0x00000010;

    private const uint NIS_HIDDEN = 0x00000001;
    private const uint NIIF_INFO = 0x00000001;

    private const int IDI_INFORMATION = 32516;
    private static readonly IntPtr HWND_MESSAGE = new(-3);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct NOTIFYICONDATAW
    {
        public uint cbSize;
        public IntPtr hWnd;
        public uint uID;
        public uint uFlags;
        public uint uCallbackMessage;
        public IntPtr hIcon;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string szTip;
        public uint dwState;
        public uint dwStateMask;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string szInfo;
        public uint uTimeoutOrVersion;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string szInfoTitle;
        public uint dwInfoFlags;
        public Guid guidItem;
        public IntPtr hBalloonIcon;
    }

    [DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool Shell_NotifyIconW(uint dwMessage, ref NOTIFYICONDATAW lpData);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr CreateWindowExW(
        uint dwExStyle, string lpClassName, string lpWindowName,
        uint dwStyle, int x, int y, int width, int height,
        IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DestroyWindow(IntPtr hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr LoadIconW(IntPtr hInstance, IntPtr lpIconName);
}
