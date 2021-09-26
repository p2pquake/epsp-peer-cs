using Client.Peer;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace WpfClient.EPSPDataView
{
    public class EPSPQuakeView
    {
        public EPSPQuakeEventArgs EventArgs { get; init; }

        public Visibility ForeignIconVisibility => EventArgs.InformationType == QuakeInformationType.Foreign ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ScaleVisibility => EventArgs.InformationType == QuakeInformationType.Foreign ? Visibility.Collapsed : Visibility.Visible;

        public SolidColorBrush ScaleForeground => EventArgs.Scale.CompareTo("5") >= 0 && EventArgs.Scale.CompareTo("7") <= 0
                    ? (SolidColorBrush)App.Current.MainWindow.FindResource("SystemControlErrorTextForegroundBrush")
                    : (SolidColorBrush)App.Current.MainWindow.FindResource("SystemControlPageTextBaseHighBrush");
    }
}
