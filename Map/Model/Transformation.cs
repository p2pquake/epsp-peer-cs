using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Map.Model
{
    public record GeoCoordinate(double Latitude, double Longitude);
    public record PixelCoordinate(int X, int Y);
    public record FloatPixelCoordinate(float X, float Y);

    public class Transformation
    {
        public LTRBCoordinate LTRBCoordinate { get; init; }
        public bool IsMercator { get; init; }
        public int ImageWidth { get; init; }
        public int ImageHeight { get; init; }

        public FloatPixelCoordinate Geo2FloatPixel(GeoCoordinate geoCoordinate)
        {
            // 割合を求める
            double latitudeRate = 1 - (geoCoordinate.Latitude - LTRBCoordinate.BottomLatitude) / (LTRBCoordinate.TopLatitude - LTRBCoordinate.BottomLatitude);
            double longitudeRate = (geoCoordinate.Longitude - LTRBCoordinate.LeftLongitude) / (LTRBCoordinate.RightLongitude - LTRBCoordinate.LeftLongitude);

            if (IsMercator)
            {
                // メルカトルの割合
                double top = Mercator(LTRBCoordinate.TopLatitude);
                double bottom = Mercator(LTRBCoordinate.BottomLatitude);
                double current = Mercator(geoCoordinate.Latitude);
                latitudeRate = 1 - (current - bottom) / (top - bottom);
            }

            return new FloatPixelCoordinate(
                (float)(longitudeRate * ImageWidth),
                (float)(latitudeRate * ImageHeight)
            );
        }

        public PixelCoordinate Geo2Pixel(GeoCoordinate geoCoordinate)
        {
            // 割合を求める
            double latitudeRate = 1 - (geoCoordinate.Latitude - LTRBCoordinate.BottomLatitude) / (LTRBCoordinate.TopLatitude - LTRBCoordinate.BottomLatitude);
            double longitudeRate = (geoCoordinate.Longitude - LTRBCoordinate.LeftLongitude) / (LTRBCoordinate.RightLongitude - LTRBCoordinate.LeftLongitude);

            if (IsMercator)
            {
                // メルカトルの割合
                double top = Mercator(LTRBCoordinate.TopLatitude);
                double bottom = Mercator(LTRBCoordinate.BottomLatitude);
                double current = Mercator(geoCoordinate.Latitude);
                latitudeRate = 1 - (current - bottom) / (top - bottom);
            }

            return new PixelCoordinate(
                (int)(longitudeRate * ImageWidth),
                (int)(latitudeRate * ImageHeight)
            );
        }

        private double Mercator(double latitude)
        {
            // 6357 log(tan((π/180(90+36))/2))
            return (6357 * Math.Log(Math.Tan((Math.PI/180*(90+latitude))/2)));
        }
    }
}
