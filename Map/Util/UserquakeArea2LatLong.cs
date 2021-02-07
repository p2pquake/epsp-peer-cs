using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Map.Util
{
    public class UserquakeArea2LatLong
    {
        private static Dictionary<string, double[]> areaDictionary;

        static UserquakeArea2LatLong()
        {
            areaDictionary = new Dictionary<string, double[]>();

            string areaText = Resource.UserquakeAreas.Replace("\r", "");
            string[] areas = areaText.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string area in areas)
            {
                string[] items = area.Split(',');
                areaDictionary.Add(items[0], 
                    new double[] { double.Parse(items[4]), double.Parse(items[5]) });
            }
        }

        public static double[] convert(string name)
        {
            if (areaDictionary.ContainsKey(name))
            {
                return areaDictionary[name];
            }

            return null;
        }
    }
}
