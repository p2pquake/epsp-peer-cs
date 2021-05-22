using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Map.Model
{
    public record UserquakePoint(string Areacode, double Confidence);

    class UserquakeDrawer : AbstractDrawer
    {
        public IList<UserquakePoint> UserquakePoints { get; init; }
        private const int drawSize = 16;

        public override LTRBCoordinate CalcDrawLTRB()
        {
            throw new NotImplementedException();
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

            foreach (var point in UserquakePoints.OrderBy(e => e.Confidence))
            {
                var coordinate = uqAreas.Get(point.Areacode);
                if (coordinate == null)
                {
                    continue;
                }

                var pos = trans.Geo2Pixel(coordinate);
                var rect = new Rectangle(pos.X - (drawSize / 2 + 1), pos.Y - (drawSize / 2 + 1), drawSize + 2, drawSize + 2);
                Image.Mutate(x => x.Fill(convConfidenceColor(point.Confidence), rect));
                Image.Mutate(x => x.DrawImage(uqImages[convConfidence(point.Confidence)], new Point(pos.X - (drawSize / 2), pos.Y - (drawSize / 2)), 1));
            }
        }

        private string convConfidence(double confidence)
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

        private Color convConfidenceColor(double confidence)
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
                    224
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
