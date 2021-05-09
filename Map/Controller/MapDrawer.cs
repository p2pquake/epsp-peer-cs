using Map.Model;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;

namespace Map.Controller
{
    public record Coordinate(double Latitude, double Longitude);

    public class MapDrawer
    {
        public bool Trim { get; set; }
        public MapType MapType { get; set; }
        public Coordinate Hypocenter { get; set; }
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

            //   震度
            if (ObservationPoints != null)
            {
                drawers.Add(new ObservationPointsDrawer
                {
                    Image = image,
                    IsMercator = mapData.IsMercator,
                    LTRB = mapData.LTRBCoordinate,
                    ObservationPoints = ObservationPoints,
                });
            }

            //   地震感知情報
            if (UserquakePoints != null)
            {
                drawers.Add(new UserquakeDrawer
                {
                    Image = image,
                    IsMercator = mapData.IsMercator,
                    LTRB = mapData.LTRBCoordinate,
                    UserquakePoints = UserquakePoints,
                });
            }

            // トリム処理
            if (MapType != MapType.WORLD)
            {

            }

            // 地理院タイルの出典
            if (MapType != MapType.WORLD)
            {

            }

            // PNG 出力
            var stream = new MemoryStream();
            image.SaveAsPng(stream);
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }
    }
}
