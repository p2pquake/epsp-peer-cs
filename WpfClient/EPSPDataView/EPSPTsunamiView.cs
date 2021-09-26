using Client.Peer;

using ModernWpf;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace WpfClient.EPSPDataView
{
    public class EPSPTsunamiView
    {
        public EPSPTsunamiEventArgs EventArgs { get; init; }

        public string Source
        {
            get
            {
                if (EventArgs.IsCancelled)
                {
                    return "/Resources/Icons/tsunami_gray.png";
                }

                return ThemeManager.Current.ActualApplicationTheme switch
                {
                    ApplicationTheme.Light => "/Resources/Icons/tsunami_red.png",
                    ApplicationTheme.Dark => "/Resources/Icons/tsunami_yellow.png",
                    _ => throw new NotImplementedException(),
                };
            }
        }

        public string Time => EventArgs.ReceivedAt.ToString("dd日HH時mm分");

        public string Caption
        {
            get
            {
                if (EventArgs.IsCancelled)
                {
                    return "津波予報 解除";
                }

                return MaxTsunamiCategory() switch
                {
                    TsunamiCategory.MajorWarning => "大津波警報",
                    TsunamiCategory.Warning => "津波警報",
                    TsunamiCategory.Advisory => "津波注意報",
                    _ => "津波予報"
                };
            }
        }

        private TsunamiCategory MaxTsunamiCategory()
        {
            if (EventArgs.IsCancelled) { return TsunamiCategory.Unknown; }

            var categories = EventArgs.RegionList.Select(e => e.Category).Distinct();
            if (categories.Contains(TsunamiCategory.MajorWarning)) { return TsunamiCategory.MajorWarning; }
            if (categories.Contains(TsunamiCategory.Warning)) { return TsunamiCategory.Warning; }
            if (categories.Contains(TsunamiCategory.Advisory)) { return TsunamiCategory.Advisory; }
            return TsunamiCategory.Unknown;
        }
    }
}
