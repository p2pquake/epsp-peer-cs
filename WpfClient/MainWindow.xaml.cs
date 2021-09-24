using ModernWpf.Controls;

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

namespace WpfClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public event EventHandler OnUserquake = (s, e) => { };

        public MainWindow()
        {
            InitializeComponent();
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            var viewModel = (RootViewModel)DataContext;
            var item = (NavigationViewItem)args.SelectedItem;

            viewModel.BindingDataContext = item.Tag switch
            {
                "Information" => viewModel.InformationViewModel,
                "Setting" => viewModel.SettingViewModel,
                "Status" => viewModel.StatusViewModel,
                _ => throw new NotImplementedException(),
            };
            viewModel.PageFileName = $"Pages/{item.Tag}.xaml";
        }

        private void Frame_Navigated(object sender, NavigationEventArgs e)
        {
            ((FrameworkElement)e.Content).DataContext = ((RootViewModel)DataContext).BindingDataContext;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // FIXME: ボタン操作時のフィードバックをどうするか考えてない
            OnUserquake(this, EventArgs.Empty);
        }
    }
}
