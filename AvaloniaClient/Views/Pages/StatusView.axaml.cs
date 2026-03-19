using Avalonia.Controls;
using Avalonia.Interactivity;

using System.Diagnostics;
using System.Runtime.InteropServices;

namespace AvaloniaClient.Views.Pages;

public partial class StatusView : UserControl
{
    private LicenseWindow? licenseWindow;

    public StatusView()
    {
        InitializeComponent();
    }

    private void HyperlinkButton_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string url)
        {
            OpenUrl(url);
        }
    }

    private void ShowLicenseButton_Click(object? sender, RoutedEventArgs e)
    {
        if (licenseWindow == null)
        {
            licenseWindow = new LicenseWindow();
            licenseWindow.Closed += (s, e) => { licenseWindow = null; };
        }
        licenseWindow.Show();
    }

    private static void OpenUrl(string url)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            Process.Start("xdg-open", url);
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            Process.Start("open", url);
    }
}
