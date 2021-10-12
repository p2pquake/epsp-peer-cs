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
using System.Windows.Threading;

using WpfClient.EPSPDataView;

namespace WpfClient.Pages.Informations
{
    /// <summary>
    /// EEWTest.xaml の相互作用ロジック
    /// </summary>
    public partial class EEWTest : Page
    {
        public EEWTest()
        {
            InitializeComponent();
            this.DataContextChanged += EEWTest_DataContextChanged;
            
        }

        private void EEWTest_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.DataContext is null) { return; }
            var view = (EPSPEEWTestView)this.DataContext;
            if (view.EventArgs is null) { return; }

            if (DateTime.Now.Subtract(view.EventArgs.ReceivedAt).TotalSeconds > 30) {
                Warning.Visibility = Visibility.Collapsed;
                return;
            }
            Past.Visibility = Visibility.Collapsed;

            var count = 0;
            var timer = new DispatcherTimer(DispatcherPriority.Normal)
            {
                Interval = TimeSpan.FromSeconds(0.2),
            };
            timer.Tick += (s, e) =>
            {
                count++;
                if (count > 150) {
                    FlashMessage.Foreground = Brushes.Gray;
                    timer.Stop();
                    return;
                }

                if (count % 5 == 0)
                {
                    FlashMessage.Foreground = Brushes.Gray;
                }
                if (count % 5 == 1)
                {
                    FlashMessage.Foreground = Brushes.Red;
                }
            };
            timer.Start();

            Unloaded += (s, e) => timer.Stop();
        }
    }
}
