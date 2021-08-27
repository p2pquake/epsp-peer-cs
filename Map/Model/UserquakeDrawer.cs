using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp.Drawing;

namespace Map.Model
{
    public record UserquakePoint(string Areacode, double Confidence);

    class UserquakeDrawer : AbstractDrawer
    {
        public IList<UserquakePoint> UserquakePoints { get; init; }
        private const int drawSize = 12;

        public override LTRBCoordinate CalcDrawLTRB()
        {
            if (UserquakePoints == null || UserquakePoints.Count == 0)
            {
                return null;
            }

            var uqAreas = UserquakeAreas.Instance;
            var coordinates = UserquakePoints
                .Where(e => uqAreas.ContainsKey(e.Areacode))
                .Select(e => uqAreas.Get(e.Areacode));

            return new LTRBCoordinate(
                coordinates.Select(e => e.Longitude).Min(),
                coordinates.Select(e => e.Latitude).Max(),
                coordinates.Select(e => e.Longitude).Max(),
                coordinates.Select(e => e.Latitude).Min()
            );
        }

        public override void Draw()
        {
            var uqAreas = UserquakeAreas.Instance;
            var uqImages = new Dictionary<string, Image>()
            {
                { "A", Image.Load(new MemoryStream(Map.ImageResource.ConfidenceA)) },
                { "B", Image.Load(new MemoryStream(Map.ImageResource.ConfidenceB)) },
                { "C", Image.Load(new MemoryStream(Map.ImageResource.ConfidenceC)) },
                { "D", Image.Load(new MemoryStream(Map.ImageResource.ConfidenceD)) },
                { "E", Image.Load(new MemoryStream(Map.ImageResource.ConfidenceE)) },
                { "F", Image.Load(new MemoryStream(Map.ImageResource.ConfidenceF)) },
            };
            uqImages.Values.ToList().ForEach(e => e.Mutate(x => x.Resize(drawSize, drawSize)));

            var trans = new Transformation
            {
                ImageWidth = Image.Width,
                ImageHeight = Image.Height,
                IsMercator = IsMercator,
                LTRBCoordinate = LTRB,
            };

            // TODO: 処理が若干冗長。。
            // 塗りつぶし
            foreach (var point in UserquakePoints.OrderBy(e => e.Confidence))
            {
                if (!uqAreas.ContainsMultiPolygonKey(point.Areacode))
                {
                    continue;
                }

                var coordinatesArray = uqAreas.GetMultiPolygon(point.Areacode);

                // 輪郭線
                foreach (var coordinates in coordinatesArray)
                {
                    var path = new PathBuilder().AddLines(coordinates.Select(e =>
                    {
                        var pos = trans.Geo2FloatPixel(e);
                        return new SixLabors.ImageSharp.PointF(pos.X, pos.Y);
                    })).Build();

                    Image.Mutate(x => x.Draw(new Pen(Color.Gray.WithAlpha(0.5f), 2), path));
                }

                // 塗りつぶし
                foreach (var coordinates in coordinatesArray)
                {
                    var path = new PathBuilder().AddLines(coordinates.Select(e =>
                    {
                        var pos = trans.Geo2FloatPixel(e);
                        return new SixLabors.ImageSharp.PointF(pos.X, pos.Y);
                    })).Build();

                    Image.Mutate(x => x.Fill(ConvConfidenceColor(point.Confidence).WithAlpha(0.95f), path));
                }
            }

            // 文字描画
            foreach (var point in UserquakePoints.OrderBy(e => e.Confidence))
            {
                if (!uqAreas.ContainsKey(point.Areacode))
                {
                    continue;
                }
                var coordinate = uqAreas.Get(point.Areacode);
                var pos = trans.Geo2Pixel(coordinate);
                var rect = new Rectangle(pos.X - (drawSize / 2 + 1), pos.Y - (drawSize / 2 + 1), drawSize + 2, drawSize + 2);
                Image.Mutate(x => x.DrawImage(uqImages[ConvConfidence(point.Confidence)], new Point(pos.X - (drawSize / 2), pos.Y - (drawSize / 2)), 1));
            }
        }

        private string ConvConfidence(double confidence)
        {
            return confidence switch
            {
                var n when n > 0.8 => "A",
                var n when n > 0.6 => "B",
                var n when n > 0.4 => "C",
                var n when n > 0.2 => "D",
                var n when n > 0.0 => "E",
                _ => "F"
            };
        }

        private Color ConvConfidenceColor(double confidence)
        {
            if (confidence < 0)
            {
                return Color.FromRgba(192, 192, 192, 128);
            }

            if (confidence >= 0.5)
            {
                var multiply = (confidence - 0.5) * 2;
                return Color.FromRgba(
                    (byte)(244 + (multiply * -4)),
                    (byte)(160 + (multiply * -32)),
                    (byte)(64 + (multiply * -64)),
                    255
                    );
            }
            else
            {
                var multiply = confidence * 2;
                return Color.FromRgba(
                        (byte)(255 + (multiply * -11)),
                        (byte)(248 + (multiply * -88)),
                        (byte)(240 + (multiply * -176)),
                        192
                    );
            }
        }
    }
}
