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

        if (listBox.SelectedIndex < 0 || listBox.SelectedIndex >= context.Histories.Count)
            return;

        context.ActiveEventArgs = context.Histories[listBox.SelectedIndex];
        System.Diagnostics.Debug.WriteLine($"リスト選択変更: {context.ActiveEventArgs?.GetType().Name}");
        
        if (context.ActiveEventArgs is EPSPQuakeEventArgs qea)
        {
            context.ActiveViewModel = new EarthquakeViewModel(qea, context);
            System.Diagnostics.Debug.WriteLine("EarthquakeViewModel作成");
        }
        else if (context.ActiveEventArgs is EPSPEEWEventArgs eea)
        {
            context.ActiveViewModel = new EEWViewModel(eea, context);
            System.Diagnostics.Debug.WriteLine("EEWViewModel作成");
        }
        else if (context.ActiveEventArgs is UserquakeEvaluateEventArgs ueea)
        {
            context.ActiveViewModel = new UserquakeViewModel(ueea, context);
            System.Diagnostics.Debug.WriteLine("UserquakeViewModel作成");
        }
        else if (context.ActiveEventArgs is EPSPTsunamiEventArgs tea)
        {
            var tsunamiViewModel = new TsunamiViewModel();
            tsunamiViewModel.Initialize(tea);
            context.ActiveViewModel = tsunamiViewModel;
            System.Diagnostics.Debug.WriteLine("TsunamiViewModel作成（手動選択）");
        }
        
        System.Diagnostics.Debug.WriteLine($"ActiveViewModel設定: {context.ActiveViewModel?.GetType().Name}");
    }

    private void ContentControl_SizeChanged(object? sender, Avalonia.Controls.SizeChangedEventArgs e)
    {
        var context = (InformationViewModel)DataContext;
        context.BodyWidth = e.NewSize.Width;
        context.BodyHeight = e.NewSize.Height;
    }
}