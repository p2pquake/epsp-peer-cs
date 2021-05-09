using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Map.Model
{
    class HypocenterDrawer : AbstractDrawer
    {
        public double Latitude { get; init; }
        public double Longitude { get; init; }

        public override LTRBCoordinate CalcDrawLTRB()
        {
            return new LTRBCoordinate(Longitude, Latitude, Longitude, Latitude);
        }

        public override void Draw()
        {
            var transformation = new Transformation
            {
                ImageWidth = Image.Width,
                ImageHeight = Image.Height,
                IsMercator = IsMercator,
                LTRBCoordinate = LTRB,
            };
            var pos = transformation.Geo2Pixel(new GeoCoordinate(Latitude, Longitude));

            var size = 6;
            var thickness = 2;

            new int[] { -1, 1 }.ToList().ForEach(v =>
            {
                var path = new Path(new LinearLineSegment(
                    new SixLabors.ImageSharp.PointF(pos.X - size, pos.Y - (size * v)),
                    new SixLabors.ImageSharp.PointF(pos.X + size, pos.Y + (size * v))
                ));
                Image.Mutate(x => x.Draw(Color.Red, thickness, path));
            });
        }
    }
}
