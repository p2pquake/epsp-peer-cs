using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

using AvaloniaClient.ViewModels;

namespace AvaloniaClient.Views.Pages;

public partial class InformationView : UserControl
{
    public InformationView()
    {
        InitializeComponent();
    }

    private void ListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not InformationViewModel viewModel) return;
        if (sender is ListBox listBox)
        {
            viewModel.SelectItem = listBox.SelectedItem;
        }
    }

    private void DetailContent_SizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (DataContext is not InformationViewModel viewModel) return;
        viewModel.FrameWidth = e.NewSize.Width;
        viewModel.FrameHeight = e.NewSize.Height;
    }

    private void TutorialPanel_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is not InformationViewModel viewModel) return;
        viewModel.TutorialVisible = false;
    }
}
