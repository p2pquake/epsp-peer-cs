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
        

        public PointCalculator(double latitude_top, double latitude_bottom, double longitude_left, double longitude_right) :
            this(latitude_top, latitude_bottom, longitude_left, longitude_right, 1, 1)
        {
        }

        public PointCalculator(double latitude_top, double latitude_bottom, double longitude_left, double longitude_right, double width, double height)
        {
            latitude = new double[] { latitude_top, latitude_bottom };
            longitude = new double[] { longitude_left, longitude_right };
            this.width = width;
            this.height = height;
        }

        public double[] calculate(double latitude, double longitude)
        {
            // 割合を求める
            double lat_rate = 1 - (latitude - this.latitude[1]) / (this.latitude[0] - this.latitude[1]);
            double longi_rate = (longitude - this.longitude[0]) / (this.longitude[1] - this.longitude[0]);

            return new double[] { longi_rate * width, lat_rate * height };
        }

        public int[] calculateInt(double latitude, double longitude)
        {
            double[] point = calculate(latitude, longitude);
            return new int[] { (int)Math.Round(point[0], MidpointRounding.AwayFromZero), (int)Math.Round(point[1], MidpointRounding.AwayFromZero) };
        }
    }
}
