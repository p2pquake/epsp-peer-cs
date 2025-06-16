using Avalonia.Controls;
using MsBox.Avalonia;

using AvaloniaUIClient.Mediator;
using AvaloniaUIClient.ViewModels;

namespace AvaloniaUIClient.Views
{
    public partial class MainWindow : Window
    {
        //private List<ViewModelBase> viewModels;

        public MainWindow()
        {
            InitializeComponent();
            var viewModel = new MainWindowViewModel(
                new InformationViewModel()
                );
            DataContext = viewModel;

            // 「揺れた！」送信結果の通知イベントを購読
            viewModel.OnUserquakeSent += ShowUserquakeResult;

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
            else if (listBox.SelectedIndex == 1)
            {
                context.ActiveViewModel = context.ConfigurationViewModel;
            }
        }

        private async void ShowUserquakeResult(string message)
        {
            // Avalonia UI でメッセージボックスを表示
            var messageBox = MessageBoxManager
                .GetMessageBoxStandard("P2P地震情報", message, MsBox.Avalonia.Enums.ButtonEnum.Ok);
            await messageBox.ShowAsync();
        }
    }
}