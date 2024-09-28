using Avalonia.Controls;

using AvaloniaUIClient.Mediator;
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
            var viewModel = new MainWindowViewModel(
                new InformationViewModel()
                );
            DataContext = viewModel;

            var reloader = new HistoryReloader(
                viewModel.InformationViewModel,
                viewModel.Mediator.MediatorContext
                );
            reloader.ReloadByApi();
            //viewModel.Mediator.Start();
        }

        private void ListBox_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
        {
            var context = (MainWindowViewModel)DataContext;
            var listBox = (ListBox)sender;

            if (listBox.SelectedIndex == 0)
            {
                context.ActiveViewModel = context.InformationViewModel;
            }
        }
    }
}