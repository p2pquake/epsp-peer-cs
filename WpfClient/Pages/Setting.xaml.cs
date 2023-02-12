using ModernWpf.Controls;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

        private void TestEarthquakeButton_Click(object sender, RoutedEventArgs e)
        {
            TestButton(sender, PublishType.Earthquake);
        }

        private void TestUserquakeButton_Click(object sender, RoutedEventArgs e)
        {
            TestButton(sender, PublishType.Userquake);
        }

        private void TestTsunamiButton_Click(object sender, RoutedEventArgs e)
        {
            TestButton(sender, PublishType.Tsunami);
        }

        private void TestEEWButton_Click(object sender, RoutedEventArgs e)
        {
            TestButton(sender, PublishType.EEW);
        }

        private async void TestButton(object sender, PublishType publishType)
        {
            var button = (Button)sender;
            button.IsEnabled = false;
            await Task.Delay(5000);
            button.IsEnabled = true;

            var viewModel = (SettingViewModel)DataContext;
            var context = viewModel.MediatorContext;
            var scale = viewModel.EarthquakeMinScale.Replace("震度", "").Replace("以上", "").Replace(" ", "");
            switch (publishType)
            {
                case PublishType.Earthquake:
                    context.TestEarthquake(scale); break;
                case PublishType.Userquake:
                    context.TestUserquake(); break;
                case PublishType.Tsunami:
                    context.TestTsunami(); break;
                case PublishType.EEW:
                    context.TestEEW(); break;
            }
        }

        enum PublishType
        {
            Earthquake,
            Userquake,
            Tsunami,
            EEW
        }
    }
}
