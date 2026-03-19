using Avalonia.Controls;
using Avalonia.Interactivity;

using FluentAvalonia.UI.Controls;

using AvaloniaClient.ViewModels;

using System;

namespace AvaloniaClient.Views;

public partial class MainWindow : Window
{
    public event EventHandler OnExit = (s, e) => { };
    public event EventHandler OnUserquake = (s, e) => { };

    private bool isExiting = false;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void ShowWindow()
    {
        Show();
        if (WindowState == WindowState.Minimized)
        {
            WindowState = WindowState.Normal;
        }
        Activate();
    }

    private void NavigationView_SelectionChanged(object? sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (DataContext is not RootViewModel viewModel) return;
        if (args.SelectedItem is not NavigationViewItem item) return;

        viewModel.CurrentPage = item.Tag?.ToString() switch
        {
            "Information" => viewModel.InformationViewModel,
            "Setting" => viewModel.SettingViewModel,
            "Status" => viewModel.StatusViewModel,
            _ => viewModel.InformationViewModel,
        };
    }

    private void ShakeButton_Click(object? sender, RoutedEventArgs e)
    {
        OnUserquake(this, EventArgs.Empty);
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (!isExiting)
        {
            e.Cancel = true;
            Hide();
            return;
        }
        base.OnClosing(e);
    }

    public void ExitApplication()
    {
        isExiting = true;
        OnExit(this, EventArgs.Empty);
    }
}
