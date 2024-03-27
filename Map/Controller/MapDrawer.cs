using Map.Model;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using Size = System.Drawing.Size;
using SizeF = System.Drawing.SizeF;

namespace Map.Controller
{
    public class MapDrawer
    {
        /// <summary>凡例や出典を表示しない</summary>
        public bool HideNote { get; set; }
        /// <summary>希望するアスペクト比率 (N:1 の N 部分)</summary>
        public double PreferedAspectRatio { get; set; }
        /// <summary>描画しない（トリム・凡例表示のみ）</summary>
        public bool HideDraw { get; set; }
        public bool Trim { get; set; }
        public MapType MapType { get; set; }
        public GeoCoordinate Hypocenter { get; set; }
        public IList<ObservationPoint> ObservationPoints { get; init; }
        public IList<UserquakePoint> UserquakePoints { get; init; }
        public IList<TsunamiPoint> TsunamiPoints { get; init; }
        public IList<Areapeer> Areapeers { get; init; }
        public IList<EEWPoint> EEWPoints { get; init; }

        public MapDrawer()
        {
            ObservationPoints = new List<ObservationPoint>();
            UserquakePoints = new List<UserquakePoint>();
            TsunamiPoints = new List<TsunamiPoint>();
            Areapeers = new List<Areapeer>();
            EEWPoints = new List<EEWPoint>();
        }

        public MemoryStream DrawAsPng()
        {
            // 画像ロード
            var mapData = MapLoader.Load(MapType);
            var image = mapData.Image;

            // 描画対象を準備 ----
            var drawers = new List<AbstractDrawer>();

            //   震度
            if (ObservationPoints != null && ObservationPoints.Any())
            {
                drawers.Add(new ObservationAreasDrawer
                {
                    Image = image,
                    IsMercator = mapData.IsMercator,
                    LTRB = mapData.LTRBCoordinate,
                    ObservationPoints = ObservationPoints,
                });
                drawers.Add(new ObservationPointsDrawer
                {
                    Image = image,
                    IsMercator = mapData.IsMercator,
                    LTRB = mapData.LTRBCoordinate,
                    ObservationPoints = ObservationPoints,
                });
            }

            // 緊急地震速報（警報）
            if (EEWPoints != null && EEWPoints.Any())
            {
                drawers.Add(new EEWDrawer
                {
                    Image = image,
                    IsMercator = mapData.IsMercator,
                    LTRB = mapData.LTRBCoordinate,
                    EEWPoints = EEWPoints,
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

            //   津波予報
            if (TsunamiPoints != null && TsunamiPoints.Any())
            {
                drawers.Add(new TsunamiAreasDrawer
                {
                    Image = image,
                    IsMercator = mapData.IsMercator,
                    LTRB = mapData.LTRBCoordinate,
                    TsunamiPoints = TsunamiPoints,
                });
            }

            // ピア分布図
            if (Areapeers != null && Areapeers.Any())
            {
                drawers.Add(new AreapeersDrawer
                {
                    Image = image,
                    IsMercator = mapData.IsMercator,
                    LTRB = mapData.LTRBCoordinate,
                    Areapeers = Areapeers,
                });
            }

            // 描画処理
            if (!HideDraw)
            {
                drawers.ForEach(x => x.Draw());
            }

            // トリム処理
            if (MapType != MapType.WORLD_512 && MapType != MapType.WORLD_1024 && Trim)
            {
                var trans = new Transformation
                {
                    ImageWidth = image.Width,
                    ImageHeight = image.Height,
                    IsMercator = mapData.IsMercator,
                    LTRBCoordinate = mapData.LTRBCoordinate
                };

                Dictionary<MapType, Size> minimumMap = new()
                {
                    { MapType.JAPAN_1024, new Size(240, 160) },
                    { MapType.JAPAN_2048, new Size(320, 240) },
                    { MapType.JAPAN_4096, new Size(320, 240) },
                    { MapType.JAPAN_8192, new Size(1920, 1280) },
                };

                Dictionary<MapType, SizeF> paddingMap = new()
                {
                    { MapType.JAPAN_1024, new SizeF(1.0f, 0.75f) },
                    { MapType.JAPAN_2048, new SizeF(1.0f, 0.75f) },
                    { MapType.JAPAN_4096, new SizeF(0.75f, 0.5f) },
                    { MapType.JAPAN_8192, new SizeF(1.0f, 0.75f) },
                };

                var coordinates = drawers.Select(e => e.CalcDrawLTRB()).Where(e => e != null);
                var lt = trans.Geo2Pixel(new GeoCoordinate(
                    coordinates.Select(e => e.TopLatitude).Max() + paddingMap[MapType].Height,
                    coordinates.Select(e => e.LeftLongitude).Min() - paddingMap[MapType].Width
                ));
                var rb = trans.Geo2Pixel(new GeoCoordinate(
                    coordinates.Select(e => e.BottomLatitude).Min() - paddingMap[MapType].Height,
                    coordinates.Select(e => e.RightLongitude).Max() + paddingMap[MapType].Width
                ));

                var marginX = MapType == MapType.JAPAN_1024 ? 320 : MapType == MapType.JAPAN_2048 ? 360 : 480;
                var marginY = MapType == MapType.JAPAN_1024 ? 120 : MapType == MapType.JAPAN_2048 ? 160 : 240;
                var l = new int[] { 0, lt.X }.Max();
                var t = new int[] { 0, lt.Y }.Max();
                var r = new int[] { image.Width, rb.X }.Min();
                var b = new int[] { image.Height, rb.Y }.Min();

                var minimum = minimumMap[MapType];
                if (r - l < minimum.Width)
                {
                    l = new int[] { 0, l - minimum.Width /2 }.Max();
                    r = new int[] { image.Width, r + minimum.Width /2 }.Min();
                }
                if (b - t < minimum.Height)
                {
                    t = new int[] { 0,  t - minimum.Height /2 }.Max();
                    b = new int[] { image.Height, b + minimum.Height /2 }.Min();
                }

                if (PreferedAspectRatio > 0)
                {
                    var ratio = (double)(r - l) / (b - t);
                    if (PreferedAspectRatio > ratio)
                    {
                        // 横を増やす
                        var appendWidth = (b - t) * PreferedAspectRatio - (r - l);
                        l = new int[] { 0, (int)(l - appendWidth / 2) }.Max();
                        r = new int[] { image.Width, (int)(r + appendWidth / 2) }.Min();
                    } else
                    {
                        // 縦を増やす
                        var appendHeight = (r - l) / PreferedAspectRatio - (b - t);
                        t = new int[] { 0, (int)(t - appendHeight / 2) }.Max();
                        b = new int[] { image.Height, (int)(b + appendHeight / 2) }.Min();
                    }
                }

                image.Mutate(x => x.Crop(new Rectangle(l, t, r - l, b - t)));
            }

            // 地理院タイルの出典
            if (MapType != MapType.WORLD_512 && MapType != MapType.WORLD_1024 && !HideNote)
            {
                using var desc = Image.Load(new MemoryStream(Map.ImageResource.description));
                image.Mutate(x => x.DrawImage(desc, new Point(0, image.Height - desc.Height), 1));
            }

            // 地震感知情報の凡例
            if (UserquakePoints != null && UserquakePoints.Any() && !HideNote)
            {
                using var uqNote = Image.Load(new MemoryStream(Map.ImageResource.UserquakeNote));
                uqNote.Mutate(x => x.Resize(uqNote.Width / 5, uqNote.Height / 5));
                image.Mutate(x => x.DrawImage(uqNote, new Point(image.Width - uqNote.Width - 8, image.Height - uqNote.Height - 8), 1));
            }

            // 地震情報の凡例
            if (MapType != MapType.WORLD_512 && MapType != MapType.WORLD_1024 && Hypocenter != null && ObservationPoints != null && ObservationPoints.Any() && !HideNote)
            {
                using var qNote = Image.Load(new MemoryStream(Map.ImageResource.QuakeNote));
                if (MapType == MapType.JAPAN_1024)
                {
                    qNote.Mutate(x => x.Resize(qNote.Width / 6, qNote.Height / 6));
                } else
                {
                    qNote.Mutate(x => x.Resize(qNote.Width / 5, qNote.Height / 5));
                }
                image.Mutate(x => x.DrawImage(qNote, new Point(image.Width - qNote.Width - 8, image.Height - qNote.Height - 8), 1));
            }

            // 津波予報の凡例
            if (TsunamiPoints != null && TsunamiPoints.Any() && !HideNote)
            {
                var tsunamiNoteResource = Map.ImageResource.TsunamiNoteMajorWarning;
                if (TsunamiPoints.Any(e => e.Category == TsunamiCategory.MajorWarning))
                {
                    tsunamiNoteResource = Map.ImageResource.TsunamiNoteMajorWarning;
                }
                else if (TsunamiPoints.Any(e => e.Category == TsunamiCategory.Warning))
                {
                    tsunamiNoteResource = Map.ImageResource.TsunamiNoteWarning;
                }
                else
                {
                    tsunamiNoteResource = Map.ImageResource.TsunamiNoteAdvisory;
                }

                using var tsunamiNote = Image.Load(new MemoryStream(tsunamiNoteResource));
                tsunamiNote.Mutate(x => x.Resize(tsunamiNote.Width / 5, tsunamiNote.Height / 5));
                image.Mutate(x => x.DrawImage(tsunamiNote, new Point(image.Width - tsunamiNote.Width - 8, image.Height - tsunamiNote.Height - 8), 1));
            }

            // PNG 出力
            var stream = new MemoryStream();
            image.SaveAsPng(stream, new SixLabors.ImageSharp.Formats.Png.PngEncoder
            {
                ColorType = SixLabors.ImageSharp.Formats.Png.PngColorType.Rgb,
                CompressionLevel = SixLabors.ImageSharp.Formats.Png.PngCompressionLevel.BestSpeed,
                FilterMethod = SixLabors.ImageSharp.Formats.Png.PngFilterMethod.None
            });
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }
    }
}
