using Map.Model;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Map.Controller
{
    public class MapDrawer
    {
        public bool Trim { get; set; }
        public MapType MapType { get; set; }
        public GeoCoordinate Hypocenter { get; set; }
        public IList<ObservationPoint> ObservationPoints { get; init; }
        public IList<UserquakePoint> UserquakePoints { get; init; }

        public MapDrawer()
        {
            ObservationPoints = new List<ObservationPoint>();
            UserquakePoints = new List<UserquakePoint>();
        }

        public Stream DrawAsPng()
        {
            // 画像ロード
            var mapData = MapLoader.Load(MapType);
            var image = mapData.Image;

            // 描画対象を準備 ----
            var drawers = new List<AbstractDrawer>();

            //   震度
            if (ObservationPoints != null && ObservationPoints.Any())
            {
                drawers.Add(new ObservationPointsDrawer
                {
                    Image = image,
                    IsMercator = mapData.IsMercator,
                    LTRB = mapData.LTRBCoordinate,
                    ObservationPoints = ObservationPoints,
                });
            }

            //   震源
            if (Hypocenter != null)
            {
                drawers.Add(new HypocenterDrawer {
                    Image = image,
                    IsMercator = mapData.IsMercator,
                    LTRB = mapData.LTRBCoordinate,
                    Latitude = Hypocenter.Latitude,
                    Longitude = Hypocenter.Longitude,
                });
            }

            //   地震感知情報
            if (UserquakePoints != null && UserquakePoints.Any())
            {
                drawers.Add(new UserquakeDrawer
                {
                    Image = image,
                    IsMercator = mapData.IsMercator,
                    LTRB = mapData.LTRBCoordinate,
                    UserquakePoints = UserquakePoints,
                });
            }

            // 描画処理
            drawers.ForEach(x => x.Draw());

            // トリム処理
            if (MapType != MapType.WORLD)
            {

            }

            // 地理院タイルの出典
            if (MapType != MapType.WORLD)
            {
                using var desc = Image.Load(new MemoryStream(Map.ImageResource.description));
                image.Mutate(x => x.DrawImage(desc, new Point(0, image.Height - desc.Height), 1));
            }

            // 地震感知情報の凡例
            if (UserquakePoints != null && UserquakePoints.Any())
            {
                using var uqNote = Image.Load(new MemoryStream(Map.ImageResource.UserquakeNote));
                uqNote.Mutate(x => x.Resize(uqNote.Width / 4, uqNote.Height / 4));
                image.Mutate(x => x.DrawImage(uqNote, new Point(image.Width - uqNote.Width - 8, image.Height - uqNote.Height - 8), 1));
            }

            // 地震情報の凡例
            if (Hypocenter != null)
            {
                using var qNote = Image.Load(new MemoryStream(Map.ImageResource.QuakeNote));
                qNote.Mutate(x => x.Resize(qNote.Width / 4, qNote.Height / 4));
                image.Mutate(x => x.DrawImage(qNote, new Point(image.Width - qNote.Width - 8, image.Height - qNote.Height - 8), 1));
            }

            // PNG 出力
            var stream = new MemoryStream();
            image.SaveAsPng(stream);
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }
    }
}
