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

            if (item is EPSPEEWView eew)
            {
                if (eew.EventArgs.IsCancelled)
                {
                    return element.FindResource("EEWCancelledItem") as DataTemplate;
                }
                return element.FindResource("EEWItem") as DataTemplate;
            }

            if (item is EPSPUserquakeView)
            {
                return element.FindResource("UserquakeItem") as DataTemplate;
            }

            return null;
        }
    }
}
