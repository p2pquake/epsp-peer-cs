using Map.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace EPSPWPFClient.Userquake
{
    class UQDrawer
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

            if (dictionary == null)
            {
                return;
            }

            var calculator = new PointCalculator(latlong[0], latlong[1], latlong[2], latlong[3], img.ActualWidth, img.ActualHeight);
            var offsetX = (canvas.ActualWidth - img.ActualWidth) / 2;
            var offsetY = (canvas.ActualHeight - img.ActualHeight) / 2;

            foreach (var kv in areaPoints)
            {
                if (!dictionary.ContainsKey(kv.Key))
                {
                    continue;
                }

                var point = kv.Value;
                var value = dictionary[kv.Key];

                var xy = calculator.calculate(point[0], point[1]);

                var borderColor = Brushes.Black;
                var bgColor = new SolidColorBrush(Color.FromArgb(255, 230, 204, 255));

                var control = new ContentControl()
                {
                    Content = new Border()
                    {
                        BorderBrush = borderColor,
                        BorderThickness = new System.Windows.Thickness(1),
                        Child = new TextBlock()
                        {
                            Text = value.ToString(),
                            FontSize = 12,
                            Foreground = Brushes.Black,
                            Background = bgColor,
                        },
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
