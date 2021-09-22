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
        public MainWindow()
        {
            InitializeComponent();
        }

        private void NavigationView_SelectionChanged(ModernWpf.Controls.NavigationView sender, ModernWpf.Controls.NavigationViewSelectionChangedEventArgs args)
        {
            Visibility info = Visibility.Hidden;
            Visibility setting = Visibility.Hidden;
            Visibility status = Visibility.Hidden;

            if (args.SelectedItem is NavigationViewItem item)
            {
                switch (item.Content)
                {
                    case "情報":
                        info = Visibility.Visible;
                        break;
                    case "設定":
                        setting = Visibility.Visible;
                        break;
                    default:
                        status = Visibility.Visible;
                        break;
                }
            }

            var context = (ViewModel)this.DataContext;
            context.ShowInfo = info;
            context.ShowSetting = setting;
            context.ShowStatus = status;
        }
    }
}
