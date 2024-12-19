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
using System.Globalization;

namespace Map.Model
{
    public record EEWPoint(string Areacode);

    class EEWDrawer : AbstractDrawer
    {
        public IList<EEWPoint> EEWPoints { get; init; }

        public override LTRBCoordinate CalcDrawLTRB()
        {
            if (EEWPoints == null || EEWPoints.Count == 0)
            {
                return null;
            }

            // XXX: かなり雑である
            //      緊急地震速報（警報）の府県予報区地域コード → 震度情報で用いる区域名 → 緯度経度
            var eewAreas = EEWAreas.Instance;
            var stations = Stations.Instance;
            var eqAreaCode2Name = PointResource.EarthquakeAreaCodes.Replace("\r", "").Split('\n').Where((line) => line.Length > 0).Select((line) => line.Split(',')).ToDictionary(e => e[0], e => e[1]);

            var names = EEWPoints
                .Where(e => eewAreas.ContainsKey(e.Areacode.ToString()))
                .Select(e => eewAreas.GetEQAreaCodes(e.Areacode.ToString()).ToList())
                .SelectMany(e => e).Where(e => eqAreaCode2Name.ContainsKey(e)).Select(e => eqAreaCode2Name[e]);

            var coordinates = names
                .Select(e => stations.GetArea(e)).Where(e => e != null);

            return new LTRBCoordinate(
                coordinates.Select(e => e.Longitude).Min(),
                coordinates.Select(e => e.Latitude).Max(),
                coordinates.Select(e => e.Longitude).Max(),
                coordinates.Select(e => e.Latitude).Min()
            );
        }

        public override void Draw()
        {
            var eewAreas = EEWAreas.Instance;

            var trans = new Transformation
            {
                ImageWidth = Image.Width,
                ImageHeight = Image.Height,
                IsMercator = IsMercator,
                LTRBCoordinate = LTRB,
            };

            // 塗りつぶし
            foreach (var point in EEWPoints)
            {
                if (!eewAreas.ContainsMultiPolygonKey(point.Areacode))
                {
                    continue;
                }

                var coordinatesArray = eewAreas.GetMultiPolygon(point.Areacode);
                var paths = coordinatesArray.Select(coordinates => 
                    new PathBuilder().AddLines(coordinates.Select(e =>
                    {
                        var pos = trans.Geo2FloatPixel(e);
                        return new SixLabors.ImageSharp.PointF(pos.X, pos.Y);
                    })).Build()
                ).ToArray();
                var lineSize = Image.Width > 1024 ? 8 : 4;

                // 輪郭線
                foreach (var path in paths)
                {
                    Image.Mutate(x => x.Draw(new Pen(Color.DarkOrange.WithAlpha(0.8f), lineSize), path));
                }

                // 塗りつぶし
                foreach (var path in paths)
                {
                    Image.Mutate(x => x.Fill(Color.Orange.WithAlpha(0.95f), path));
                }
            }

        }
    }
}
