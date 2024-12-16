using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Map.Model
{
    class ObservationAreasDrawer : AbstractDrawer
    {
        public IList<ObservationPoint> ObservationPoints { get; init; }

        public override LTRBCoordinate CalcDrawLTRB()
        {
            return null;
        }

        static readonly Dictionary<int, Color> colorMap = new Dictionary<int, Color>
        {
            { 10, new Color(new Rgb24(160, 224, 255)) },
            { 20, new Color(new Rgb24(160, 208, 255)) },
            { 30, new Color(new Rgb24(176, 192, 255)) },
            { 40, new Color(new Rgb24(112, 224, 128)) },
            { 45, new Color(new Rgb24(128, 192,   0)) },
            { 46, new Color(new Rgb24(128, 192,   0)) },
            { 50, new Color(new Rgb24(240, 128,   0)) },
            { 55, new Color(new Rgb24(208, 112,   0)) },
            { 60, new Color(new Rgb24(224,  32,  32)) },
            { 70, new Color(new Rgb24(160,   0,  32)) }
        };

        public override void Draw()
        {
            var areas = ObservationAreas.Instance;

            var trans = new Transformation
            {
                ImageWidth = Image.Width,
                ImageHeight = Image.Height,
                IsMercator = IsMercator,
                LTRBCoordinate = LTRB,
            };

            foreach (var point in ObservationPoints.OrderBy(e => e.Scale))
            {
                var area = areas.GetArea(point.Name);
                if (area == null)
                {
                    continue;
                }

                if (!colorMap.ContainsKey(point.Scale)) {
                    continue;
                }

                foreach (var coordinates in area.Coordinates)
                {
                    var path = new PathBuilder().AddLines(coordinates.Select(e => {
                        var pos = trans.Geo2FloatPixel(e);
                        return new SixLabors.ImageSharp.PointF(pos.X, pos.Y);
                    })).Build();
                    Image.Mutate(x => x.Fill(colorMap[point.Scale].WithAlpha(0.7f), path));
                }
            }
        }
    }
}
