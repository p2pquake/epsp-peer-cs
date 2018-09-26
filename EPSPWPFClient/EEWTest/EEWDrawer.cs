using Client.Peer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EPSPWPFClient.EEWTest
{
    class EEWDrawer
    {
        public EPSPEEWTestEventArgs EventArgs { private get; set; }

        public void Redraw(Canvas canvas)
        {
            Draw(canvas);
        }

        public void Draw(Canvas canvas)
        {
            canvas.Children.Clear();

            var uri = "pack://application:,,,/Resources/japan.png";
            var latlong = new int[] { 47, 23, 121, 150 };

            // 地図描画
            var img = new Image();
            img.Source = new BitmapImage(new Uri(uri));
            img.Width = canvas.ActualWidth;
            img.Height = canvas.ActualHeight;
            canvas.Children.Add(img);
            img.InvalidateMeasure();
            img.UpdateLayout();

            // 描画位置
            var offsetX = (canvas.ActualWidth - img.ActualWidth) / 2;
            var offsetY = (canvas.ActualHeight - img.ActualHeight) / 2;

            {
                var control = new ContentControl()
                {
                    Content = new Rectangle() { Width = img.ActualWidth, Height = img.ActualHeight, Fill = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0)) }
                };
                Canvas.SetLeft(control, offsetX + 0);
                Canvas.SetTop(control, offsetY);
                canvas.Children.Add(control);
            }

            var message = "緊急地震速報 配信試験(β)\n\n発表を検出しました。";

            {
                var text = new TextBlock() { Text = message, FontSize = 18, Foreground = Brushes.Black };
                var control = new ContentControl() { Content = text };
                Canvas.SetLeft(control, offsetX + 0);
                Canvas.SetTop(control, offsetY + 1);
                canvas.Children.Add(control);
            }

            {
                var text = new TextBlock() { Text = message, FontSize = 18, Foreground = Brushes.White };
                var control = new ContentControl() { Content = text };
                Canvas.SetLeft(control, offsetX - 1);
                Canvas.SetTop(control, offsetY);
                canvas.Children.Add(control);
            }
        }
    }
}
