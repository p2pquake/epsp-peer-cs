using Avalonia.Controls;
using Avalonia.Interactivity;

using FluentAvalonia.UI.Controls;

using AvaloniaClient.ViewModels;

using System.Threading.Tasks;

namespace AvaloniaClient.Views.Pages;

public partial class SettingView : UserControl
{
    public SettingView()
    {
        InitializeComponent();
    }

    private void NavigationView_SelectionChanged(object? sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (DataContext is not SettingViewModel viewModel) return;
        if (args.SelectedItem is not NavigationViewItem item) return;
        viewModel.SelectTag = item.Tag?.ToString();
    }

    private void TestEarthquakeButton_Click(object? sender, RoutedEventArgs e)
    {
        TestButton(TestEarthquakeButton, PublishType.Earthquake);
    }

    private void TestUserquakeButton_Click(object? sender, RoutedEventArgs e)
    {
        TestButton(TestUserquakeButton, PublishType.Userquake);
    }

    private void TestTsunamiButton_Click(object? sender, RoutedEventArgs e)
    {
        TestButton(TestTsunamiButton, PublishType.Tsunami);
    }

    private void TestEEWButton_Click(object? sender, RoutedEventArgs e)
    {
        TestButton(TestEEWButton, PublishType.EEW);
    }

    private async void TestButton(Button button, PublishType publishType)
    {
        if (DataContext is not SettingViewModel viewModel) return;
        var context = viewModel.MediatorContext;
        if (context == null) return;

        button.IsEnabled = false;
        await Task.Delay(5000);
        button.IsEnabled = true;

        var scale = viewModel.EarthquakeMinScale?.Replace("震度", "").Replace("以上", "").Replace(" ", "") ?? "3";
        switch (publishType)
        {
            case PublishType.Earthquake:
                context.TestEarthquake(scale); break;
            case PublishType.Userquake:
                context.TestUserquake(); break;
            case PublishType.Tsunami:
                context.TestTsunami(); break;
            case PublishType.EEW:
                context.TestEEW(); break;
        }
    }

    private enum PublishType
    {
        Earthquake,
        Userquake,
        Tsunami,
        EEW,
    }
}
