using ModernWpf.Controls;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
    /// Setting.xaml の相互作用ロジック
    /// </summary>
    public partial class Setting : System.Windows.Controls.Page
    {
        public Setting()
        {
            InitializeComponent();
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            var viewModel = (SettingViewModel)DataContext;
            var item = (NavigationViewItem)args.SelectedItem;

            viewModel.SelectTag = item.Tag.ToString();
        }
    }
}
