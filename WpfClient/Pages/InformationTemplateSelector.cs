using Client.App.Userquake;
using Client.Peer;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using WpfClient.EPSPDataView;

namespace WpfClient.Pages
{
    public class InformationTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var element = container as FrameworkElement;

            if (item is EPSPQuakeView)
            {
                return element.FindResource("EarthquakeItem") as DataTemplate;
            }

            if (item is EPSPTsunamiView)
            {
                return element.FindResource("TsunamiItem") as DataTemplate;
            }

            if (item is EPSPEEWTestView)
            {
                return element.FindResource("EEWTestItem") as DataTemplate;
            }

            if (item is EPSPUserquakeView)
            {
                return element.FindResource("UserquakeItem") as DataTemplate;
            }

            return null;
        }
    }
}
