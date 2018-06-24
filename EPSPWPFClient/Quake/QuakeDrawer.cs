using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Client.Peer;
using EPSPWPFClient.Properties;
using Map.Map;
using Map.Util;
using SharpVectors.Converters;
using SharpVectors.Renderers.Wpf;
using SharpVectors.Runtime;

namespace EPSPWPFClient.Quake
{
    class QuakeDrawer
    {
        public EPSPQuakeEventArgs QuakeEventArgs { private get; set; }
        
        public void Redraw(Canvas canvas)
        {
            Draw(canvas);
        }
        
        public void Draw(Canvas canvas)
        {
            canvas.Children.Clear();

            // 地図描画
            var img = new Image();
            img.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/japan.png"));
            img.Width = canvas.ActualWidth;
            img.Height = canvas.ActualHeight;
            canvas.Children.Add(img);
            img.InvalidateMeasure();
            img.UpdateLayout();

            // 震度観測点描画のための準備
            var calculator = new PointCalculator(47, 23, 121, 150,
                img.ActualWidth, img.ActualHeight);

            var offsetX = (canvas.ActualWidth - img.ActualWidth) / 2;
            var offsetY = (canvas.ActualHeight - img.ActualHeight) / 2;

            var scaleImageList = new List<BitmapImage>()
            {
                new BitmapImage(new Uri("pack://application:,,,/Resources/Scale/1.png")),
                new BitmapImage(new Uri("pack://application:,,,/Resources/Scale/2.png")),
                new BitmapImage(new Uri("pack://application:,,,/Resources/Scale/3.png")),
                new BitmapImage(new Uri("pack://application:,,,/Resources/Scale/4.png")),
                new BitmapImage(new Uri("pack://application:,,,/Resources/Scale/5l.png")),
                new BitmapImage(new Uri("pack://application:,,,/Resources/Scale/5u.png")),
                new BitmapImage(new Uri("pack://application:,,,/Resources/Scale/6l.png")),
                new BitmapImage(new Uri("pack://application:,,,/Resources/Scale/6u.png")),
                new BitmapImage(new Uri("pack://application:,,,/Resources/Scale/7.png")),
                new BitmapImage(new Uri("pack://application:,,,/Resources/Scale/unknown.png"))
            };

            var scaleMap = new Dictionary<string, int>()
            {
                { "1", 0 },
                { "2", 1 },
                { "3", 2 },
                { "4", 3 },
                { "5弱", 4 },
                { "5強", 5 },
                { "6弱", 6 },
                { "6強", 7 },
                { "7", 8 }
            };

            // 震度観測点描画
            if (QuakeEventArgs == null || QuakeEventArgs.PointList == null)
            {
                return;
            }
                 
            foreach (var point in QuakeEventArgs.PointList)
            {
                var latLong = PointName2LatLong.convert(point.Name);

                if (latLong == null)
                {
                    continue;
                }

                var xy = calculator.calculate(latLong[0], latLong[1]);

                var image = new Image();
                if (scaleMap.ContainsKey(point.Scale))
                {
                    image.Source = scaleImageList[scaleMap[point.Scale]];
                } else
                {
                    image.Source = scaleImageList.Last();
                }
                image.Width = 16;
                image.Height = 16;

                Canvas.SetLeft(image, xy[0] + offsetX - 8);
                Canvas.SetTop(image, xy[1] + offsetY - 8);
                canvas.Children.Add(image);
            }

            // 震源を描画
            if (QuakeEventArgs.InformationType == QuakeInformationType.Destination ||
                QuakeEventArgs.InformationType == QuakeInformationType.Detail ||
                QuakeEventArgs.InformationType == QuakeInformationType.Foreign ||
                QuakeEventArgs.InformationType == QuakeInformationType.ScaleAndDestination)
            {
                var xy = calculator.calculate(
                    double.Parse(QuakeEventArgs.Latitude.Replace("N", "").Replace("S", "-")),
                    double.Parse(QuakeEventArgs.Longitude.Replace("E", "").Replace("W", "-"))
                    );
                var image = new Image()
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/Resources/hypocenter.png")),
                };
                image.Width = 16;
                image.Height = 16;

                Canvas.SetLeft(image, xy[0] + offsetX - 8);
                Canvas.SetTop(image, xy[1] + offsetY - 8);
                canvas.Children.Add(image);
            }

            // 文字描画
            var textList = new List<string>();
            textList.Add(string.Format(" {1} （{0}）", CodeMapper.ToString(QuakeEventArgs.InformationType), QuakeEventArgs.OccuredTime));
            textList.Add("");
            if (QuakeEventArgs.InformationType == QuakeInformationType.Detail ||
                QuakeEventArgs.InformationType == QuakeInformationType.ScaleAndDestination ||
                QuakeEventArgs.InformationType == QuakeInformationType.ScalePrompt)
            {
                textList.Add("最大震度 " + QuakeEventArgs.Scale);
            }
            if (QuakeEventArgs.InformationType == QuakeInformationType.Destination ||
                QuakeEventArgs.InformationType == QuakeInformationType.Detail ||
                QuakeEventArgs.InformationType == QuakeInformationType.Foreign ||
                QuakeEventArgs.InformationType == QuakeInformationType.ScaleAndDestination)
            {
                textList.Add("震源 " + QuakeEventArgs.Destination);
                textList.Add("深さ " + QuakeEventArgs.Depth);
                textList.Add("規模 M" + QuakeEventArgs.Magnitude);
            }
            textList.Add(CodeMapper.ToString(QuakeEventArgs.TsunamiType));

            {
                TextBlock text = new TextBlock() { Text = string.Join("\n ", textList), FontSize = 14, Foreground = Brushes.Black, Background = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0)) };
                ContentControl control = new ContentControl() { Content = text };
                Canvas.SetLeft(control, offsetX + 0);
                Canvas.SetTop(control, offsetY + 1);
                canvas.Children.Add(control);
            }

            {
                TextBlock text = new TextBlock() { Text = string.Join("\n ", textList), FontSize = 14, Foreground = Brushes.White };
                ContentControl control = new ContentControl() { Content = text };
                Canvas.SetLeft(control, offsetX - 1);
                Canvas.SetTop(control, offsetY);
                canvas.Children.Add(control);
            }
        }
    }
}
