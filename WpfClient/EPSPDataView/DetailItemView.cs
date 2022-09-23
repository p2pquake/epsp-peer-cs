using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfClient.EPSPDataView
{
    public enum TextStyles
    {
        Title,
        Name,
        // 地震情報
        Prefecture,
        Scale,
        // 津波予報
        Section,
        MajorWarning,
        Warning,
        Advisory,
    }

    public class DetailItemView
    {
        public string Text { get; init; }
        public TextStyles TextStyle { get; init; }
        public string ScaleIconPath { get; init; }

        public DetailItemView(string text, TextStyles style, int scale = -1)
        {
            this.Text = text;
            this.TextStyle = style;
            this.ScaleIconPath = $"/Resources/Scales/scale_{scale}.png";
        }
    }
}
