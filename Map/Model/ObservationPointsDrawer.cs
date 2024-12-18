using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;

namespace Map.Model
{
    public record ObservationPoint(string Prefecture, string Name, int Scale);

    class ObservationPointsDrawer : AbstractDrawer
    {
        public IList<ObservationPoint> ObservationPoints { get; init; }
        private ObservationAreas areas = ObservationAreas.Instance;

        public override LTRBCoordinate CalcDrawLTRB()
        {
            if (ObservationPoints == null || ObservationPoints.Count == 0)
            {
                return null;
            }

            var stations = Stations.Instance;
            var coordinates = ObservationPoints
                .Select(e => stations.GetPoint(e.Name, e.Prefecture))
                .Where(e => e != null);

            return new LTRBCoordinate(
                coordinates.Select(e => e.Longitude).Min(),
                coordinates.Select(e => e.Latitude).Max(),
                coordinates.Select(e => e.Longitude).Max(),
                coordinates.Select(e => e.Latitude).Min()
            );
        }

        public override void Draw()
        {
            var drawPointSize = Image.Width > 1024 ? 16 : 12;
            var drawAreaSize = Image.Width > 1024 ? 24 : 16;

            var stations = Stations.Instance;
            // XXX: かしこくない実装方法
            var scaleImages = new Dictionary<int, Image>() {
                { 10, Image.Load(new MemoryStream(Map.ImageResource.Scale10)) },
                { 20, Image.Load(new MemoryStream(Map.ImageResource.Scale20)) },
                { 30, Image.Load(new MemoryStream(Map.ImageResource.Scale30)) },
                { 40, Image.Load(new MemoryStream(Map.ImageResource.Scale40)) },
                { 45, Image.Load(new MemoryStream(Map.ImageResource.Scale45)) },
                { 46, Image.Load(new MemoryStream(Map.ImageResource.Scale46)) },
                { 50, Image.Load(new MemoryStream(Map.ImageResource.Scale50)) },
                { 55, Image.Load(new MemoryStream(Map.ImageResource.Scale55)) },
                { 60, Image.Load(new MemoryStream(Map.ImageResource.Scale60)) },
                { 70, Image.Load(new MemoryStream(Map.ImageResource.Scale70)) },
            };
            scaleImages.Values.ToList().ForEach(e => e.Mutate(x => x.Resize(drawPointSize, drawPointSize)));
            var scaleAreaImages = new Dictionary<int, Image>() {
                { 10, Image.Load(new MemoryStream(Map.ImageResource.Scale10)) },
                { 20, Image.Load(new MemoryStream(Map.ImageResource.Scale20)) },
                { 30, Image.Load(new MemoryStream(Map.ImageResource.Scale30)) },
                { 40, Image.Load(new MemoryStream(Map.ImageResource.Scale40)) },
                { 45, Image.Load(new MemoryStream(Map.ImageResource.Scale45)) },
                { 46, Image.Load(new MemoryStream(Map.ImageResource.Scale46)) },
                { 50, Image.Load(new MemoryStream(Map.ImageResource.Scale50)) },
                { 55, Image.Load(new MemoryStream(Map.ImageResource.Scale55)) },
                { 60, Image.Load(new MemoryStream(Map.ImageResource.Scale60)) },
                { 70, Image.Load(new MemoryStream(Map.ImageResource.Scale70)) },
            };
            scaleAreaImages.Values.ToList().ForEach(e => e.Mutate(x => x.Resize(drawAreaSize, drawAreaSize)));

            var trans = new Transformation
            {
                ImageWidth = Image.Width,
                ImageHeight = Image.Height,
                IsMercator = IsMercator,
                LTRBCoordinate = LTRB,
            };

            foreach (var point in ObservationPoints.OrderBy(e => e.Scale))
            {
                var coordinate = stations.GetPoint(point.Name, point.Prefecture);
                if (coordinate == null)
                {
                    continue;
                }

                var drawSize = areas.GetArea(point.Name) == null ? drawPointSize : drawAreaSize;
                var scaleImage = areas.GetArea(point.Name) == null ? scaleImages[point.Scale] : scaleAreaImages[point.Scale];

                var pos = trans.Geo2Pixel(coordinate);
                var rect = new Rectangle(pos.X - (drawSize / 2 + 1), pos.Y - (drawSize / 2 + 1), drawSize + 2, drawSize + 2);
                Image.Mutate(x => x.Fill(Color.Black, rect));
                Image.Mutate(x => x.DrawImage(scaleImage, new Point(pos.X - (drawSize / 2), pos.Y - (drawSize / 2)), 1));
            }
        }
    }
}
