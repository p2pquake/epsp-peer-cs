using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

        //private void RefreshButton_Click(object sender, RoutedEventArgs e)
        //{
        //    Program.RefreshInformation();
        //}
    }
}
