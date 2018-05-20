using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace EPSPWPFClient.ViewModel
{
    internal class StateConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value.Length < 2 || !(value[0] is string) || !(value[1] is int))
            {
                return null;
            }

            string state = (string)value[0];
            int connections = (int)value[1];

            var dictionary = new Dictionary<string, string[]>()
            {
                { "ConnectedState",     new string[] { "接続済み", "status_connected.png" } },
                { "ConnectingState",    new string[] { "接続中", "status_connecting.png" } },
                { "DisconnectedState",  new string[] { "未接続", "status_disconnected.png" } },
                { "DisconnectingState", new string[] { "切断中", "status_disconnected.png" } },
                { "MaintenanceState",   new string[] { "接続済み", "status_connected.png" } }
            };

            var selectedValue = new string[] { "???", "status_disconnected.png" };

            if (dictionary.ContainsKey(state))
            {
                selectedValue = dictionary[state];
            }

            if (selectedValue[1] == "status_connected.png" && connections == 0)
            {
                selectedValue = new string[] { selectedValue[0], "status_connecting.png" };
            }

            if (targetType.Name == "String")
            {
                return selectedValue[0];
            }

            return new BitmapImage(new Uri("Resources/Pictogram/" + selectedValue[1], UriKind.Relative));
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
