using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

namespace WpfClient.Pages
{
    /// <summary>
    /// Information.xaml の相互作用ロジック
    /// </summary>
    public partial class Information : Page
    {
        public Information()
        {
            InitializeComponent();
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var viewModel = (InformationViewModel)DataContext;
            viewModel.SelectItem = ((ModernWpf.Controls.ListView)sender).SelectedItem;
        }

        private void Frame_Navigated(object sender, NavigationEventArgs e)
        {
            ((FrameworkElement)e.Content).DataContext = ((InformationViewModel)DataContext).SelectItem;
        }

        private void Frame_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var viewModel = (InformationViewModel)DataContext;
            viewModel.FrameWidth = e.NewSize.Width;
            viewModel.FrameHeight = e.NewSize.Height;
        }

        private void TutorialPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var viewModel = (InformationViewModel)DataContext;
            viewModel.TutorialVisibility = Visibility.Hidden;
        }

        private void Frame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode != NavigationMode.New) { e.Cancel = true; }
        }

        //private void RefreshButton_Click(object sender, RoutedEventArgs e)
        //{
        //    Program.RefreshInformation();
        //}
    }
}
