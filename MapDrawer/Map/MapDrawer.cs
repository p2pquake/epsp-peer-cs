using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Map.Map
{
    class MapDrawer
    {
        private int x1;
        private int x2;
        private int y1;
        private int y2;

        public Rectangle DrawBound
        {
            get
            {
                if (x1 > x2 || y1 > y2) {
                    return new Rectangle(0, 0, Bitmap.Width, Bitmap.Height);
                }
                return new Rectangle(x1, y1, x2 - x1, y2 - y1);
            }
        }
        public Bitmap TrimmedBitmap {
            get {
                // DrawBoundに10%余白を足してIntersectする
                var drawBound = DrawBound;
                var xMargin = (int)(drawBound.Width * 0.1) + 120;
                var yMargin = (int)(drawBound.Height * 0.1) + 120;
                drawBound.X -= xMargin;
                drawBound.Y -= yMargin;
                drawBound.Width += xMargin * 2;
                drawBound.Height += yMargin * 2;

                var intersectArea = Rectangle.Intersect(new Rectangle(0, 0, Bitmap.Width, Bitmap.Height), drawBound);
                var destBitmap = new Bitmap(intersectArea.Width, intersectArea.Height, Bitmap.PixelFormat);
                var g = Graphics.FromImage(destBitmap);
                var destRectangle = new Rectangle(0, 0, intersectArea.Width, intersectArea.Height);
                g.DrawImage(Bitmap, destRectangle, intersectArea, GraphicsUnit.Pixel);
                g.Dispose();
                return destBitmap;
            }
        }
        public Bitmap Bitmap { get; }
        Graphics g;
        PointCalculator pointCalculator;

        Brush brush10 = new SolidBrush(Color.FromArgb(160, 224, 255));
        Brush brush20 = new SolidBrush(Color.FromArgb(160, 208, 255));
        Brush brush30 = new SolidBrush(Color.FromArgb(176, 192, 255));
        Brush brush40 = new SolidBrush(Color.FromArgb(112, 224, 128));
        Brush brush45 = new SolidBrush(Color.FromArgb(128, 192,   0));
        Brush brush50 = new SolidBrush(Color.FromArgb(240, 128,   0));
        Brush brush55 = new SolidBrush(Color.FromArgb(208, 112,   0));
        Brush brush60 = new SolidBrush(Color.FromArgb(224,  32,  32));
        Brush brush70 = new SolidBrush(Color.FromArgb(160,   0,  32));
        Font font = new Font(FontFamily.GenericMonospace, 9);

        public MapDrawer(string fileName, double[] latitude_coverage, double[] longitude_coverage, bool is_mercator)
        {
            Bitmap = new Bitmap(Image.FromFile(fileName));
            g = Graphics.FromImage(Bitmap);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            pointCalculator = new PointCalculator(
                latitude_coverage[0], latitude_coverage[1],
                longitude_coverage[0], longitude_coverage[1],
                Bitmap.Width, Bitmap.Height, is_mercator
                );

            x1 = int.MaxValue;
            x2 = int.MinValue;
            y1 = int.MaxValue;
            y2 = int.MinValue;
        }

        public void drawHypocenter(double latitude, double longitude)
        {
            int penSize = 2;
            int drawSize = 6;

            int[] point = pointCalculator.calculateInt(latitude, longitude);
            Pen pen = new Pen(Color.Red, penSize);
            Point point1 = new Point(point[0] - drawSize + 1, point[1] - drawSize + 1);
            Point point2 = new Point(point[0] + drawSize, point[1] + drawSize);
            Point point3 = new Point(point[0] - drawSize + 1, point[1] + drawSize);
            Point point4 = new Point(point[0] + drawSize, point[1] - drawSize + 1);
            g.DrawLine(pen, point1, point2);
            g.DrawLine(pen, point3, point4);

            UpdateBound(point[0], point[1]);
        }

        public void drawPoint(double latitude, double longitude, int scale)
        {
            Brush brush = Brushes.Aqua;

            // FIXME: この実装はイマイチすぎる。せめてマップ。
            if (scale == 10)
                brush = brush10;
            if (scale == 20)
                brush = brush20;
            if (scale == 30)
                brush = brush30;
            if (scale == 40)
                brush = brush40;
            if (scale == 45 || scale == 46)
                brush = brush45;
            if (scale == 50)
                brush = brush50;
            if (scale == 55)
                brush = brush55;
            if (scale == 60)
                brush = brush60;
            if (scale == 70)
                brush = brush70;

            drawPoint(latitude, longitude, brush, convertScale(scale));
        }

        public void drawArea(double latitude, double longitude, double confidence)
        {
            /*
             * P2PQ_Client (VB6) の定義
             * Const LOW_R = 255: Const LOW_G = 255: Const LOW_B = 255
             * Const MID_R = 244: Const MID_G = 160: Const MID_B = 64
             * Const HIGH_R = 240: Const HIGH_G = 128: Const HIGH_B = 0
             */

            Color color;
            if (confidence >= 0.5)
            {
                var multiply = (confidence - 0.5) * 2;
                color = Color.FromArgb(
                    255,
                    244 + (int)(multiply * -4),
                    160 + (int)(multiply * -32),
                    64 + (int)(multiply * -64)
                    );
            }
            else
            {
                var multiply = confidence * 2;
                color = Color.FromArgb(
                    255,
                    255 + (int)(multiply * -11),
                    255 + (int)(multiply * -95),
                    255 + (int)(multiply * -191)
                    );
            }

            int drawSize = 32 + (int)(64 * confidence);
            int[] point = pointCalculator.calculateInt(latitude, longitude);

            GraphicsPath path = new GraphicsPath();
            path.AddEllipse(point[0] - (drawSize / 2), point[1] - (drawSize / 2), drawSize, drawSize);
            PathGradientBrush brush = new PathGradientBrush(path)
            {
                CenterColor = color,
                SurroundColors = new Color[] { Color.FromArgb(0, color.R, color.G, color.B) }
            };

            g.FillEllipse(brush,
                point[0] - (drawSize / 2), point[1] - (drawSize / 2),
                drawSize, drawSize);

            string drawString = "E";
            if (confidence > 0.2) { drawString = "D"; }
            if (confidence > 0.4) { drawString = "C"; }
            if (confidence > 0.6) { drawString = "B"; }
            if (confidence > 0.8) { drawString = "A"; }

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    g.DrawString(drawString, font, Brushes.Black, point[0] - 4 + dx, point[1] - 4 + dy);
                }
            }
            g.DrawString(drawString, font, Brushes.White, point[0] - 4, point[1] - 4);

            UpdateBound(point[0], point[1]);
        }

        private string convertScale(int scale)
        {
            Dictionary<int, string> scaleDictionary = new Dictionary<int, string>()
            {
                {10, "1"},
                {20, "2"},
                {30, "3"},
                {40, "4"},
                {45, "5-"},
                {50, "5+"},
                {55, "6-"},
                {60, "6+"},
                {70, "7"},
                {46, "" }
            };

            return scaleDictionary[scale];
        }

        private void drawPoint(double latitude, double longitude, Brush brush, string text)
        {
            int drawSize = 12;

            int[] point = pointCalculator.calculateInt(latitude, longitude);
            g.FillRectangle(brush,
                point[0] - (drawSize / 2), point[1] - (drawSize / 2),
                drawSize, drawSize);
            g.DrawRectangle(Pens.Black,
                point[0] - (drawSize / 2), point[1] - (drawSize / 2),
                drawSize, drawSize);

            g.DrawString(text, font, Brushes.Black, point[0]-4, point[1] + (drawSize / 2)-14);

            UpdateBound(point[0], point[1]);
        }

        private void UpdateBound(int x, int y)
        {
            if (x1 > x) {
                x1 = x;
            }
            if (y1 > y) {
                y1 = y;
            }
            if (x > x2) {
                x2 = x;
            }
            if (y > y2) {
                y2 = y;
            }
        }
    }
}
