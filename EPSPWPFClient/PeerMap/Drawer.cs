using Map.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace EPSPWPFClient.PeerMap
{
    class Drawer
    {
        static Dictionary<string, double[]> areaPoints;

        internal static void Draw(Canvas canvas, IDictionary<string, int> dictionary)
        {
            canvas.Children.Clear();

            var uri = "pack://application:,,,/Resources/japan.png";
            var latlong = new int[] { 47, 23, 121, 150 };

            // 地図描画
            var img = new Image
            {
                Source = new BitmapImage(new Uri(uri)),
                Width = canvas.ActualWidth,
                Height = canvas.ActualHeight
            };
            canvas.Children.Add(img);
            img.InvalidateMeasure();
            img.UpdateLayout();
            
            InitAreaPoints();

            var calculator = new PointCalculator(latlong[0], latlong[1], latlong[2], latlong[3], img.ActualWidth, img.ActualHeight);
            var offsetX = (canvas.ActualWidth - img.ActualWidth) / 2;
            var offsetY = (canvas.ActualHeight - img.ActualHeight) / 2;

            foreach (var kv in areaPoints)
            {
                var point = kv.Value;
                var value = 0;

                if (dictionary != null && dictionary.ContainsKey(kv.Key))
                {
                    value = dictionary[kv.Key];
                }

                var xy = calculator.calculate(point[0], point[1]);

                var control = new ContentControl()
                {
                    Content = new TextBlock()
                    {
                        Text = value.ToString(),
                        FontSize = 14,
                        Foreground = Brushes.Black,
                        Background = new SolidColorBrush(Color.FromArgb(128, 255, 255, 255))
                    }
                };
                Canvas.SetLeft(control, xy[0] + offsetX - 8);
                Canvas.SetTop(control, xy[1] + offsetY - 8);
                canvas.Children.Add(control);
            }
        }

        private static void InitAreaPoints()
        {
            if (areaPoints != null)
            {
                return;
            }

            areaPoints = new Dictionary<string, double[]>();

            var lines = MapResource.EPSPArea.Split('\n');
            foreach (var line in lines.Where(e => e.Split(',').Length >= 6))
            {
                var elements = line.Split(',');
                areaPoints.Add(elements[0], new double[] { double.Parse(elements[4]), double.Parse(elements[5]) });
            }
        }
    }
}
