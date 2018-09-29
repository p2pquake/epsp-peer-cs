using Map.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Map.Util
{
    public class PointName2LatLong
    {
        private static Dictionary<string, double[]> pointDictionary;

        static PointName2LatLong()
        {
            pointDictionary = new Dictionary<string, double[]>();

            string pointText = Resource.ObservationPoints;
            string[] points = pointText.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string point in points)
            {
                string[] items = point.Split(',');
                if (pointDictionary.ContainsKey(items[1]))
                {
                    continue;
                }
                pointDictionary.Add(items[1], 
                    new double[] { double.Parse(items[2]), double.Parse(items[3]) });
            }
        }

        public static double[] convert(string name)
        {
            if (pointDictionary.ContainsKey(name))
            {
                return pointDictionary[name];
            }

            return null;
        }
    }
}
