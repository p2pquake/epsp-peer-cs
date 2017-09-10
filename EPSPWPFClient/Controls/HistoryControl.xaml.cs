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
using EPSPWPFClient.Quake;

namespace EPSPWPFClient.Controls
{
    /// <summary>
    /// HistoryControl.xaml の相互作用ロジック
    /// </summary>
    public partial class HistoryControl : UserControl
    {
        private QuakeDrawer drawer = new QuakeDrawer();


        public HistoryControl()
        {
            InitializeComponent();
        }

        public void draw()
        {
            drawer.Draw(canvas);
        }

        private void canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            drawer.Redraw(canvas);
        }
    }
}
