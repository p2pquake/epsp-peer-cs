using Avalonia.Controls;
using AvaloniaUIClient.ViewModels.Information;

namespace AvaloniaUIClient.Views.Information
{
    public partial class Tsunami : UserControl
    {
        public Tsunami()
        {
            InitializeComponent();
            SizeChanged += OnSizeChanged;
        }

        private void OnSizeChanged(object? sender, SizeChangedEventArgs e)
        {
            if (DataContext is TsunamiViewModel viewModel)
            {
                viewModel.UpdateFrameSize(e.NewSize.Width, e.NewSize.Height);
            }
        }
    }
}