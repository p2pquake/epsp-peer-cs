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

namespace EPSPWPFClient.Tsunami
{
    // TODO: インタフェース化しよ
    class TsunamiDrawer
    {
        public EPSPTsunamiEventArgs EventArgs { private get; set; }
        
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

            // 解除
            if (EventArgs.IsCancelled)
            {
                var cancelledMessage = "津波予報は解除されました。";
                
                {
                    var text = new TextBlock() { Text = cancelledMessage, FontSize = 18, Foreground = Brushes.Black };
                    var control = new ContentControl() { Content = text };
                    Canvas.SetLeft(control, offsetX + 0);
                    Canvas.SetTop(control, offsetY + 1);
                    canvas.Children.Add(control);
                }

                {
                    var text = new TextBlock() { Text = cancelledMessage, FontSize = 18, Foreground = Brushes.White };
                    var control = new ContentControl() { Content = text };
                    Canvas.SetLeft(control, offsetX - 1);
                    Canvas.SetTop(control, offsetY);
                    canvas.Children.Add(control);
                }

                return;
            }

            // 津波予報（とりあえずテキスト出力しておく）
            // TODO: あとで地図描画に対応させる
            var stream = EventArgs.RegionList.GroupBy(e => e.Category).OrderByDescending(e => e.Key).Select(e =>
            {
                var categoryMessage = new StringBuilder();
                categoryMessage.Append(
                    new Dictionary<TsunamiCategory, string>() {
                        { TsunamiCategory.MajorWarning, "【大津波警報】" },
                        { TsunamiCategory.Warning, "【津波警報】" },
                        { TsunamiCategory.Advisory, "【津波注意報】" },
                        { TsunamiCategory.Unknown, "【不明】" },
                    }[e.Key]);
                categoryMessage.Append(Environment.NewLine);
                categoryMessage.Append(
                    string.Join("、", e.Select(f => f.Region + (f.IsImmediately ? "＊" : "")).ToList())
                    );
                return categoryMessage.ToString();
            });

            var message = string.Join(Environment.NewLine, stream.ToList());

            {
                var text = new TextBlock() { Text = message, FontSize = 18, Foreground = Brushes.Black, TextWrapping = System.Windows.TextWrapping.Wrap, Width = img.ActualWidth };
                var control = new ContentControl() { Content = text };
                Canvas.SetLeft(control, offsetX + 0);
                Canvas.SetTop(control, offsetY + 1);
                canvas.Children.Add(control);
            }

            {
                var text = new TextBlock() { Text = message, FontSize = 18, Foreground = Brushes.White, TextWrapping = System.Windows.TextWrapping.Wrap, Width = img.ActualWidth };
                var control = new ContentControl() { Content = text };
                Canvas.SetLeft(control, offsetX - 1);
                Canvas.SetTop(control, offsetY);
                canvas.Children.Add(control);
            }
        }
    }
}
