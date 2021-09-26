using Client.App.Userquake;

using ModernWpf;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfClient.EPSPDataView
{
    public class EPSPUserquakeView
    {
        public UserquakeEvaluateEventArgs EventArgs { get; init; }

        public string Source => ThemeManager.Current.ActualApplicationTheme switch
        {
            ApplicationTheme.Light => "/Resources/Icons/userquake_black.png",
            ApplicationTheme.Dark => "/Resources/Icons/userquake_white.png",
            _ => throw new NotImplementedException(),
        };

        public string Time => EventArgs.StartedAt.ToString("dd日HH時mm分");

        public string Caption => "地震感知情報";
    }
}
