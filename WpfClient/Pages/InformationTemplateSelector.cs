using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WpfClient.Pages
{
    public class InformationTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var element = container as FrameworkElement;

            if (item is SampleItem sampleItem)
            {
                if (sampleItem.MaxScale.CompareTo("5") >= 0)
                {
                    return element.FindResource("SampleItemAnotherDataTemplate") as DataTemplate;
                }

                return element.FindResource("SampleItemDataTemplate") as DataTemplate;
            }

            if (item is ForeignItem)
            {
                return element.FindResource("ForeignItemDataTemplate") as DataTemplate;
            }

            return null;
        }
    }
}
