using Avalonia.Controls;

using AvaloniaUIClient.ViewModels;
using AvaloniaUIClient.ViewModels.Information;

using Client.App.Userquake;
using Client.Peer;

namespace AvaloniaUIClient.Views;

public partial class InformationView : UserControl
{
    public InformationView()
    {
        InitializeComponent();
    }

    private void ListBox_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
    {
        var context = (InformationViewModel)DataContext;
        var listBox = (ListBox)sender;

        context.ActiveEventArgs = context.Histories[listBox.SelectedIndex];
        if (context.ActiveEventArgs is EPSPQuakeEventArgs qea)
        {
            context.ActiveViewModel = new EarthquakeViewModel(qea, context);
        }
        if (context.ActiveEventArgs is EPSPEEWEventArgs eea)
        {
            context.ActiveViewModel = new EEWViewModel(eea, context);
        }
        if (context.ActiveEventArgs is UserquakeEvaluateEventArgs ueea)
        {
            context.ActiveViewModel = new UserquakeViewModel(ueea, context);
        }
    }

    private void ContentControl_SizeChanged(object? sender, Avalonia.Controls.SizeChangedEventArgs e)
    {
        var context = (InformationViewModel)DataContext;
        context.BodyWidth = e.NewSize.Width;
        context.BodyHeight = e.NewSize.Height;
    }
}