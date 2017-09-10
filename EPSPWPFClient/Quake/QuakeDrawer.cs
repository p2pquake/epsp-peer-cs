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
        private SvgViewbox svg;

        public void Redraw(Canvas canvas)
        {
            if (svg == null)
            {
                return;
            }

            svg.MaxHeight = canvas.ActualHeight;
            svg.MaxWidth = canvas.ActualWidth;
            svg.InvalidateMeasure();
            svg.UpdateLayout();
        }

        public void Draw(Canvas canvas)
        {
            canvas.Children.Clear();

            // ラスタ(ビットマップ)
            //Image img = new Image();
            //img.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/japan.png"));
            //img.Width = canvas.ActualWidth;
            //img.Height = canvas.ActualHeight;

            // ベクタ
            svg = new SvgViewbox();
            svg.MaxHeight = canvas.ActualHeight;
            svg.MaxWidth = canvas.ActualWidth;
            svg.Source = new Uri("pack://application:,,,/Resources/japan_vector.svgz");

            canvas.Children.Add(svg);

            svg.InvalidateMeasure();
            svg.UpdateLayout();

            //svgCanvas.LoadDiagrams("Resources/japan_vector.svgz");

            //canvas.Children.Add(image);

            PointCalculator calculator = new PointCalculator(47, 23, 121, 150,
                svg.ActualWidth, svg.ActualHeight);

            string[] points =
            {
                "日立市助川小学校",
                "大子町池田",
                "常陸大宮市上小瀬",
                "水戸市金町",
                "水戸市内原町",
                "日立市役所",
                "日立市十王町友部",
                "常陸太田市町屋町",
                "常陸太田市町田町",
                "常陸太田市大中町",
                "常陸太田市高柿町",
                "高萩市安良川",
                "高萩市下手綱",
                "笠間市石井",
                "笠間市中央",
                "笠間市下郷",
                "笠間市笠間",
                "ひたちなか市南神敷台",
                "ひたちなか市東石川",
                "東海村東海",
                "常陸大宮市中富町",
                "常陸大宮市北町",
                "常陸大宮市山方",
                "常陸大宮市高部",
                "常陸大宮市野口",
                "那珂市福田",
                "城里町徳蔵",
                "城里町石塚",
                "城里町阿波山",
                "小美玉市小川",
                "小美玉市堅倉",
                "小美玉市上玉里",
                "土浦市常名",
                "石岡市柿岡",
                "石岡市若宮",
                "取手市寺田",
                "牛久市城中町",
                "つくば市天王台",
                "つくば市研究学園",
                "つくば市小茎",
                "坂東市山",
                "稲敷市江戸崎甲",
                "筑西市舟生",
                "筑西市海老ヶ島",
                "筑西市門井",
                "かすみがうら市上土田",
                "かすみがうら市大和田",
                "桜川市岩瀬",
                "桜川市真壁",
                "桜川市羽田",
                "鉾田市汲上",
                "常総市水海道諏訪町"
            };

            foreach (string point in points)
            {
                double[] latLong = PointName2LatLong.convert(point);

                if (latLong == null)
                {
                    continue;
                }

                double[] xy = calculator.calculate(latLong[0], latLong[1]);

                Rectangle rectangle = new Rectangle()
                {
                    Width = 10,
                    Height = 10,
                    Fill = Brushes.Cyan,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };
                Canvas.SetLeft(rectangle, xy[0]);
                Canvas.SetTop(rectangle, xy[1]);

                canvas.Children.Add(rectangle);

                TextBlock text = new TextBlock()
                {
                    Text = "1"
                };

                ContentControl control = new ContentControl()
                {
                    Content = text
                };

                Canvas.SetLeft(control, xy[0]);
                Canvas.SetTop(control, xy[1]);

                canvas.Children.Add(control);
            }
        }
    }
}
