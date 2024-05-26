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
    public enum TsunamiCategory
    {
        MajorWarning, // 津波警報（大津波）
        Warning, // 津波警報
        Advisory, // 津波注意報
        //Slight, // 若干の海面変動
        Unknown,
    }
    public record TsunamiPoint(string Name, TsunamiCategory Category);

    public class TsunamiAreasDrawer : AbstractDrawer
    {
        public IList<TsunamiPoint> TsunamiPoints { get; init; }

        public override LTRBCoordinate CalcDrawLTRB()
        {
            if (TsunamiPoints == null || TsunamiPoints.Count == 0)
            {
                return null;
            }

            var areas = TsunamiAreas.Instance;
            var coordinates = TsunamiPoints
                .Select(e => areas.GetArea(e.Name))
                .Where(e => e != null)
                .Select(e => e.Coordinates)
                .SelectMany(e => e).SelectMany(e => e);

            return new LTRBCoordinate(
                coordinates.Min(e => e.Longitude),
                coordinates.Max(e => e.Latitude),
                coordinates.Max(e => e.Longitude),
                coordinates.Min(e => e.Latitude)
            );
        }

        public override void Draw()
        {
            var colorMap = new Dictionary<TsunamiCategory, Color>
            {
                { TsunamiCategory.MajorWarning, new Color(new Rgb24(200, 0, 255)) },
                { TsunamiCategory.Warning, new Color(new Rgb24(255, 40, 0)) },
                { TsunamiCategory.Advisory, new Color(new Rgb24(250, 245, 0)) },
            };

            var areas = TsunamiAreas.Instance;

            var trans = new Transformation
            {
                ImageWidth = Image.Width,
                ImageHeight = Image.Height,
                IsMercator = IsMercator,
                LTRBCoordinate = LTRB,
            };

            foreach (var point in TsunamiPoints.OrderBy(e => e.Category))
            {
                var area = areas.GetArea(point.Name);
                if (area == null)
                {
                    continue;
                }

                if (!colorMap.ContainsKey(point.Category))
                {
                    continue;
                }

                // TODO: 海岸線を残しながら描画するには、レイヤの概念がないと厳しそう
                foreach (var coordinates in area.Coordinates)
                {
                    var path = new PathBuilder().AddLines(coordinates.Select(e =>
                    {
                        var pos = trans.Geo2FloatPixel(e);
                        return new SixLabors.ImageSharp.PointF(pos.X, pos.Y);
                    })).Build();
                    var pen = new Pen(colorMap[point.Category], point.Category == TsunamiCategory.MajorWarning ? 12 : 6);
                    Image.Mutate(x => x.Draw(pen, path));
                }
            }
        }
    }
}
