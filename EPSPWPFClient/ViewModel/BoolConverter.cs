using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace EPSPWPFClient.ViewModel
{
    internal class BoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is bool))
            {
                return "?";
            }

            string[] viewStrings = { "○", "×" };
            if (parameter is string)
            {
                viewStrings = ((string)parameter).Split(',');
            }

            if ((bool)value)
            {
                return viewStrings[0];
            }

            return viewStrings[1];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
