using System;

namespace Map.Map
{
    /// <summary>
    /// 描画位置を算出するためのクラスです。
    /// 
    /// </summary>
    public class PointCalculator
    {
        /// <summary>
        /// 緯度
        /// </summary>
        double[] latitude;
        /// <summary>
        /// 経度
        /// </summary>
        double[] longitude;

        double width = 1;
        double height = 1;

        bool is_mercator = false;

        public PointCalculator(double latitude_top, double latitude_bottom, double longitude_left, double longitude_right) :
            this(latitude_top, latitude_bottom, longitude_left, longitude_right, 1, 1, false)
        {
        }

        public PointCalculator(double latitude_top, double latitude_bottom, double longitude_left, double longitude_right, double width, double height) :
            this(latitude_top, latitude_bottom, longitude_left, longitude_right, width, height, false)
        {
        }

        public PointCalculator(double latitude_top, double latitude_bottom, double longitude_left, double longitude_right, bool is_mercator) :
            this(latitude_top, latitude_bottom, longitude_left, longitude_right, 1, 1, is_mercator)
        {
        }

        public PointCalculator(double latitude_top, double latitude_bottom, double longitude_left, double longitude_right, double width, double height, bool is_mercator)
        {
            latitude = new double[] { latitude_top, latitude_bottom };
            longitude = new double[] { longitude_left, longitude_right };
            this.width = width;
            this.height = height;
            this.is_mercator = is_mercator;
        }

        public double[] calculate(double latitude, double longitude)
        {
            // 割合を求める
            double lat_rate = 1 - (latitude - this.latitude[1]) / (this.latitude[0] - this.latitude[1]);
            double longi_rate = (longitude - this.longitude[0]) / (this.longitude[1] - this.longitude[0]);

            if (is_mercator)
            {
                // メルカトルの割合
                double top = mercator(this.latitude[0]);
                double bottom = mercator(this.latitude[1]);
                double current = mercator(latitude);
                lat_rate = 1 - (current - bottom) / (top - bottom);
            }

            return new double[] { longi_rate * width, lat_rate * height };
        }

        public int[] calculateInt(double latitude, double longitude)
        {
            double[] point = calculate(latitude, longitude);
            return new int[] { (int)Math.Round(point[0], MidpointRounding.AwayFromZero), (int)Math.Round(point[1], MidpointRounding.AwayFromZero) };
        }

        private double mercator(double latitude)
        {
            // 6357 log(tan((π/180(90+36))/2))
            return (6357 * Math.Log(Math.Tan((Math.PI/180*(90+latitude))/2)));
        }
    }
}
