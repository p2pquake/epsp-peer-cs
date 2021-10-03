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

namespace WpfClient.Pages.Informations
{
    public class DetailDescriptionTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var element = container as FrameworkElement;

            if (item is not DetailItemView itemView)
            {
                return null;
            }

            return itemView.TextStyle switch
            {
                TextStyles.Title => element.FindResource("DetailHeaderItem") as DataTemplate,
                TextStyles.Prefecture => element.FindResource("DetailPrefectureItem") as DataTemplate,
                TextStyles.Scale => element.FindResource("DetailScaleItem") as DataTemplate,
                TextStyles.Name =>  element.FindResource("DetailNameItem") as DataTemplate,
                _ => throw new NotImplementedException(),
            };
        }
    }
}
