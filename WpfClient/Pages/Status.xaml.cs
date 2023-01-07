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
    /// Status.xaml の相互作用ロジック
    /// </summary>
    public partial class Status : Page
    {
        private LicenseWindow licenseWindow;

        public Status()
        {
            InitializeComponent();
        }

        private void ShowLicenseButton_Click(object sender, RoutedEventArgs e)
        {
            if (licenseWindow == null)
            {
                licenseWindow = new LicenseWindow();
                licenseWindow.Closed += (s, e) => { licenseWindow = null; };
            }
            licenseWindow.Show();
        }
    }
}
