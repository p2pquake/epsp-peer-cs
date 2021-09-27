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
        Prefecture,
        Scale,
        Name,
    }

    public class DetailItemView
    {
        public string Text { get; init; }
        public TextStyles TextStyle { get; init; }

        public DetailItemView(string text, TextStyles style)
        {
            this.Text = text;
            this.TextStyle = style;
        }
    }
}
