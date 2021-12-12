using SixLabors.ImageSharp;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Map.Model
{
    public enum MapType
    {
        WORLD_512,
        WORLD_1024,
        JAPAN_1024,
        JAPAN_2048,
        JAPAN_4096,
        JAPAN_8192,
    }

    /// <summary>
    /// 地図情報
    /// </summary>
    /// <param name="Image">画像データ</param>
    /// <param name="LTRBCoordinate">地図の座標範囲</param>
    /// <param name="IsMercator">メルカトル図法 (false の場合は正距円筒図法)</param>
    public record MapData(Image Image, LTRBCoordinate LTRBCoordinate, bool IsMercator);

    public static class MapLoader
    {
        /// <summary>
        /// 地図の画像データと緯度経度範囲情報を読み込みます。
        /// <para>type = MapType.JAPAN_8192 でメモリを 300 MB 程度消費します。</para>
        /// </summary>
        public static MapData Load(MapType type)
        {
            var (image, isMercator) = ImageFromResource(type);
            var ltrb = LTRB(type);
            return new MapData(image, ltrb, isMercator);
        }

        private static (Image image, bool isMercator) ImageFromResource(MapType type)
        {
            byte[] img = type switch
            {
                MapType.WORLD_512 => Map.ImageResource.world_512,
                MapType.WORLD_1024 => Map.ImageResource.world_1024,
                MapType.JAPAN_1024 => Map.ImageResource.japan_gsi_1024,
                MapType.JAPAN_2048 => Map.ImageResource.japan_gsi_2048,
                MapType.JAPAN_4096 => Map.ImageResource.japan_gsi_4096,
                MapType.JAPAN_8192 => Map.ImageResource.japan_gsi_8192,
                _ => Map.ImageResource.japan_gsi_1024,
            };
            using var imgStream = new MemoryStream(img);
            return (Image.Load(imgStream), type != MapType.WORLD_512 && type != MapType.WORLD_1024);
        }

        private static LTRBCoordinate LTRB(MapType type)
        {
            return type switch
            {
                MapType.WORLD_512 => new LTRBCoordinate(0, 90, 360, -90),
                MapType.WORLD_1024 => new LTRBCoordinate(0, 90, 360, -90),
                _ => new LTRBCoordinate(121, 47, 150, 23),
            };
        }
    }
}
