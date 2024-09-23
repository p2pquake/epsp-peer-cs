using Avalonia.Controls;

using AvaloniaUIClient.ViewModels;

using System.Collections.Generic;

namespace AvaloniaUIClient.Views
{
    public partial class MainWindow : Window
    {
        private List<ViewModelBase> viewModels;

        public MainWindow()
        {
            InitializeComponent();
            viewModels = new List<ViewModelBase>(){
                new InformationViewModel(),
                new ConfigurationViewModel(),
            };
            DataContext = new MainWindowViewModel(viewModels);
        }

        private void ListBox_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
        {
            var context = (MainWindowViewModel)DataContext;
            context.ActiveViewModel = viewModels[((ListBox)sender).SelectedIndex];
        }
    }
}