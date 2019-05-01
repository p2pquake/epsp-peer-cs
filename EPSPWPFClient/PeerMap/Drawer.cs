using Map.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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

            var totalPeer = dictionary?.Values?.Sum() ?? 0;

            foreach (var kv in areaPoints)
            {
                var point = kv.Value;
                var value = 0;

                if (dictionary != null && dictionary.ContainsKey(kv.Key))
                {
                    value = dictionary[kv.Key];
                }

                var xy = calculator.calculate(point[0], point[1]);

                var borderColor = Brushes.Gray;
                var bgColor = new SolidColorBrush(Color.FromArgb(128, 128, 128, 128));
                if (value > 0)
                {
                    borderColor = Brushes.Black;
                    bgColor = new SolidColorBrush(Color.FromArgb(128, 192, 192, 255));
                }

                var radius = Math.Min(Math.Max(0, (Math.Log10(1.0 * value / totalPeer * 20) + 1) / 2 * 128), 64);
                var ellipse = new ContentControl()
                {
                    Content = new Ellipse()
                    {
                        Fill = bgColor,
                        Width = radius,
                        Height = radius,
                    },
                };
                var textBlock = new TextBlock()
                {
                    Text = value.ToString(),
                    FontSize = 12,
                    Foreground = Brushes.Black
                };
                Canvas.SetLeft(ellipse, xy[0] + offsetX - radius/2);
                Canvas.SetTop(ellipse, xy[1] + offsetY - radius/2);
                Canvas.SetZIndex(ellipse, 3);
                canvas.Children.Add(ellipse);
                Canvas.SetLeft(textBlock, xy[0] + offsetX - 8);
                Canvas.SetTop(textBlock, xy[1] + offsetY - 8);
                Canvas.SetZIndex(textBlock, 4);
                canvas.Children.Add(textBlock);
            }

            var totalLabel = "参加総数: " + totalPeer + "ピア";
            {
                TextBlock text = new TextBlock() { Text = totalLabel, FontSize = 14, Foreground = Brushes.Black, Background = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0)) };
                ContentControl control = new ContentControl() { Content = text };
                Canvas.SetLeft(control, offsetX + 0);
                Canvas.SetTop(control, offsetY + 1);
                canvas.Children.Add(control);
            }

            {
                TextBlock text = new TextBlock() { Text = totalLabel, FontSize = 14, Foreground = Brushes.White };
                ContentControl control = new ContentControl() { Content = text };
                Canvas.SetLeft(control, offsetX - 1);
                Canvas.SetTop(control, offsetY);
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
