using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using Map.Map;
using Map.Util;
using System.IO;

namespace Map
{
    class Program
    {
        static void Main(string[] args)
        {
            string inputPath = args[0];
            double[] latitude = { double.Parse(args[2]), double.Parse(args[3]) };
            double[] longitude = { double.Parse(args[4]), double.Parse(args[5]) };
            bool is_mercator = (args.Length > 9 && args[9].Contains("mercator"));

            MapDrawer mapDrawer = new MapDrawer(inputPath, latitude, longitude, is_mercator);

            if (args.Length > 8)
            {
                StreamReader reader = new StreamReader(args[8], Encoding.UTF8);
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] items = line.Split(',');
                    double[] point = PointName2LatLong.convert(items[0]);

                    if (point == null)
                    {
                        continue;
                    }

                    mapDrawer.drawPoint(point[0], point[1], int.Parse(items[1]));
                }
            }

            mapDrawer.drawHypocenter(double.Parse(args[6]), double.Parse(args[7]));

            var bitmap = mapDrawer.Bitmap;

            if (args.Length > 9) {
                if (args[9].Contains("trim")) {
                    bitmap = mapDrawer.TrimmedBitmap;
                }
            }

            bitmap.Save(args[1], ImageFormat.Png);
        }
    }
}
