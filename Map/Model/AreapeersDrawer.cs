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
using SixLabors.Fonts;

namespace Map.Model
{
    public record Areapeer(string Areacode, int Peer);

    class AreapeersDrawer : AbstractDrawer
    {
        static readonly Lazy<FontFamily> s_fontFamily = new Lazy<FontFamily>(() =>
        {
            using var fontStream = new MemoryStream(FontResource.RobotoMono_Bold);
            var collection = new FontCollection();
            return collection.Add(fontStream);
        });
        static readonly Dictionary<int, Font> s_mappingDrawSizeToFont = new Dictionary<int, Font>();
        static Font GetFont(int drawSize)
        {
            if (!s_mappingDrawSizeToFont.TryGetValue(drawSize, out var font))
            {
                font = s_fontFamily.Value.CreateFont(drawSize, FontStyle.Bold);
                s_mappingDrawSizeToFont.Add(drawSize, font);
            }
            return font;
        }

        public IList<Areapeer> Areapeers { get; init; }

        public override LTRBCoordinate CalcDrawLTRB()
        {
            if (Areapeers == null || Areapeers.Count == 0)
            {
                return null;
            }

            var uqAreas = UserquakeAreas.Instance;
            var coordinates = Areapeers
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
            var drawSize = Image.Width > 1024 ? 24 : 12;

            var uqAreas = UserquakeAreas.Instance;

            var trans = new Transformation
            {
                ImageWidth = Image.Width,
                ImageHeight = Image.Height,
                IsMercator = IsMercator,
                LTRBCoordinate = LTRB,
            };

            foreach (var area in Areapeers)
            {
                if (!uqAreas.ContainsMultiPolygonKey(area.Areacode))
                {
                    continue;
                }

                var coordinatesArray = uqAreas.GetMultiPolygon(area.Areacode);
                var paths = coordinatesArray.Select(coordinates => new PathBuilder().AddLines(coordinates.Select(e =>
                {
                    var pos = trans.Geo2FloatPixel(e);
                    return new SixLabors.ImageSharp.PointF(pos.X, pos.Y);
                })).Build()).ToArray();

                // 輪郭線
                foreach (var path in paths)
                {
                    Image.Mutate(x => x.Draw(new Pen(Color.Black.WithAlpha(0.25f), 3), path));
                }

                // 塗りつぶし
                foreach (var path in paths)
                {
                    Image.Mutate(x => x.Fill(Color.FromRgb(160, 224, 255), path));
                }
            }

            // 文字描画
            var font = GetFont(drawSize);
            var rendererOptions = new TextOptions(font);

            foreach (var area in Areapeers) {
                if (!uqAreas.ContainsKey(area.Areacode))
                {
                    continue;
                }
                var coordinate = uqAreas.Get(area.Areacode);
                var pos = trans.Geo2Pixel(coordinate);
                var rectangle = TextMeasurer.Measure(area.Peer.ToString(), rendererOptions);
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        Image.Mutate(x => x.DrawText(area.Peer.ToString(), font, Color.Black, new PointF(pos.X - rectangle.Width / 2 + dx, pos.Y - rectangle.Height / 2 + dy)));
                    }
                }
                Image.Mutate(x => x.DrawText(area.Peer.ToString(), font, Color.White, new PointF(pos.X - rectangle.Width / 2, pos.Y - rectangle.Height / 2)));
            }
        }
    }
}
