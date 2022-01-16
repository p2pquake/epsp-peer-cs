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
using System.Windows.Threading;

using WpfClient.EPSPDataView;

namespace WpfClient.Pages.Informations
{
    /// <summary>
    /// Tsunami.xaml の相互作用ロジック
    /// </summary>
    public partial class Tsunami : Page
    {
        public Tsunami()
        {
            InitializeComponent();
            this.DataContextChanged += Tsunami_DataContextChanged;
        }

        private void Tsunami_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.DataContext is null) { return; }
            var view = (EPSPTsunamiView)this.DataContext;
            if (view.EventArgs is null) { return; }

            var count = 0;
            var timer = new DispatcherTimer(DispatcherPriority.Normal)
            {
                Interval = TimeSpan.FromSeconds(0.5),
            };
            timer.Tick += (s, e) =>
            {
                count = (count + 1) % 6;
                if (count == 0)
                {
                    FrontImage.Visibility = Visibility.Hidden;
                }
                if (count == 1)
                {
                    FrontImage.Visibility = Visibility.Visible;
                }
            };

            timer.Start();
            DataContextChanged += (s, e) => timer.Stop();
            Unloaded += (s, e) => timer.Stop();
        }
    }
}
