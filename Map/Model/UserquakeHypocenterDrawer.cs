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
using MathNet.Numerics;
using MathNet.Numerics.Optimization;

namespace Map.Model
{
    public record Userquake(DateTime Timestamp, string Areacode);

    public class EarthquakeSensorData
    {
        public DateTime Timestamp { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    class UserquakeHypocenterDrawer : AbstractDrawer
    {
        public IList<Userquake> Userquakes { get; init; }

        public override LTRBCoordinate CalcDrawLTRB()
        {
            if (Userquakes == null || Userquakes.Count == 0)
            {
                return null;
            }

            var uqAreas = UserquakeAreas.Instance;
            var coordinates = Userquakes
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

            // ChatGPT くんで出てきた震源推定
            var sensorDataList = Userquakes.Where(uq => uqAreas.ContainsKey(uq.Areacode)).GroupBy(uq => uq.Areacode).Select(uqs =>
            {
                var coordinates = uqAreas.Get(uqs.Key);
                var timestamp = uqs.OrderBy(uq => uq.Timestamp).ToArray()[uqs.Count() / 10].Timestamp;
                return new EarthquakeSensorData() { Latitude = coordinates.Latitude, Longitude = coordinates.Longitude, Timestamp = timestamp };
            }).OrderBy(uq => uq.Timestamp).ToList();

            // var geoCoordinatesUserquakes = Userquakes.Where(uq => uqAreas.ContainsKey(uq.Areacode)).Select(uq =>
            // {
            //     var coordinates = uqAreas.Get(uq.Areacode);
            //     return (coordinates.Latitude, coordinates.Longitude, uq.Timestamp);
            // }).ToArray();
            // var estimatedHypocenter = EstimateEpicenter(geoCoordinatesUserquakes);

            const double pWaveVelocity = 3.0;
            List<double> distances = CalculateDistances(sensorDataList, pWaveVelocity);

            // 震源の推定
            EarthquakeSensorData epicenter = Triangulate(sensorDataList, distances);

            Console.WriteLine($"推定した緯度経度: ({epicenter.Latitude}, {epicenter.Longitude})");
            var pos = trans.Geo2Pixel(new GeoCoordinate(epicenter.Latitude, epicenter.Longitude));

            var size = 6;
            var thickness = 2;

            new int[] { -1, 1 }.ToList().ForEach(v =>
            {
                var path = new SixLabors.ImageSharp.Drawing.Path(new LinearLineSegment(
                    new SixLabors.ImageSharp.PointF(pos.X - size, pos.Y - (size * v)),
                    new SixLabors.ImageSharp.PointF(pos.X + size, pos.Y + (size * v))
                ));
                Image.Mutate(x => x.Draw(Color.Red, thickness, path));
            });
        }

        public static List<double> CalculateDistances(List<EarthquakeSensorData> sensorDataList, double pWaveVelocity)
        {
            List<double> distances = new List<double>();

            for (int i = 0; i < sensorDataList.Count - 1; i++)
            {
                TimeSpan timeDifference = sensorDataList[i + 1].Timestamp - sensorDataList[i].Timestamp;
                double propagationTime = timeDifference.TotalSeconds;
                double distance = propagationTime * pWaveVelocity;
                distances.Add(distance);
            }

            return distances;
        }

        public static EarthquakeSensorData EstimateEpicenter(List<EarthquakeSensorData> sensorDataList, List<double> distances, double pWaveVelocity)
        {
            // 初期値はデータセットの平均値で設定
            double initialLatitude = sensorDataList.Average(x => x.Latitude);
            double initialLongitude = sensorDataList.Average(x => x.Longitude);

            // Levenberg-Marquardt法で非線形最小二乗法を適用
            var initialValues = new[] { initialLatitude, initialLongitude };
            var observations = sensorDataList.Select(x => new Tuple<double, double>(x.Latitude, x.Longitude)).ToArray();
            double[] times = sensorDataList.Select(x => (x.Timestamp - sensorDataList[0].Timestamp).TotalSeconds).ToArray();
            var solver = new LevenbergMarquardtMinimizer(gradientTolerance: 1e-8, maximumIterations: 1000);
            var optimizedParameters = solver.FindMinimum(
    (parameters, jacobian) => CalculateResiduals(parameters, observations, times, pWaveVelocity, jacobian),
    initialValues
);

            return new EarthquakeSensorData { Latitude = optimizedParameters[0], Longitude = optimizedParameters[1] };
        }

        private static double[] CalculateResiduals(double[] parameters, Tuple<double, double>[] observations, double[] times, double pWaveVelocity, double[,] jacobian)
        {
            double[] residuals = new double[observations.Length];
            double currentLatitude = parameters[0];
            double currentLongitude = parameters[1];

            for (int i = 0; i < observations.Length; i++)
            {
                double distance = HaversineDistance(currentLatitude, currentLongitude, observations[i].Item1, observations[i].Item2);
                residuals[i] = distance - pWaveVelocity * times[i];

                if (jacobian != null)
                {
                    // 数値微分によるヤコビアンの計算
                    double delta = 1e-7;
                    double[] dx = new[] { delta, 0 };
                    double[] dy = new[] { 0, delta };
                    jacobian[i, 0] = (HaversineDistance(currentLatitude + dx[0], currentLongitude + dx[1], observations[i].Item1, observations[i].Item2) - distance) / delta;
                    jacobian[i, 1] = (HaversineDistance(currentLatitude + dy[0], currentLongitude + dy[1], observations[i].Item1, observations[i].Item2) - distance) / delta;
                }
            }

            return residuals;
        }

        // ハヴァサイン法による2点間の距離計算 (単位: km)
        public static double HaversineDistance(double lat1, double lon1, double lat2, double lon2)
        {
            double R = 6371; // 地球の半径 (km)
            double dLat = ToRadians(lat2 - lat1);
            double dLon = ToRadians(lon2 - lon1);
            lat1 = ToRadians(lat1);
            lat2 = ToRadians(lat2);

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(lat1) * Math.Cos(lat2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            double distance = R * c;

            return distance;
        }

        public static double ToRadians(double angle)
        {
            return angle * (Math.PI / 180);
        }

        // static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        // {
        //     double R = 6371.0;
        //     double dLat = ToRadians(lat2 - lat1);
        //     double dLon = ToRadians(lon2 - lon1);

        //     lat1 = ToRadians(lat1);
        //     lat2 = ToRadians(lat2);

        //     double a = Math.Pow(Math.Sin(dLat / 2), 2) + Math.Cos(lat1) * Math.Cos(lat2) * Math.Pow(Math.Sin(dLon / 2), 2);
        //     double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        //     double distance = R * c;

        //     return distance;
        // }

        // static double ToRadians(double angle)
        // {
        //     return angle * (Math.PI / 180);
        // }

        // static (double latitude, double longitude, double difference) EstimateEpicenter((double, double, DateTime)[] data)
        // {
        //     double ySum = 0;
        //     double xSum = 0;
        //     double zSum = 0;
        //     double totalWeight = 0;

        //     for (int i = 0; i < data.Length; i++)
        //     {
        //         for (int j = i + 1; j < data.Length; j++)
        //         {
        //             double distance = CalculateDistance(data[i].Item1, data[i].Item2, data[j].Item1, data[j].Item2);
        //             double timeDiff = (data[j].Item3 - data[i].Item3).TotalSeconds;
        //             double weight = 1 / Math.Pow(distance, 2);
        //             Console.WriteLine($"[{data[i].Item1},{data[i].Item2}] - [{data[j].Item1},{data[j].Item2}]: distance: {distance}, timeDiff: {timeDiff}, weight: {weight}");

        //             if (double.IsInfinity(weight))
        //             {
        //                 continue;
        //             }

        //             ySum += weight * (data[i].Item1 + data[j].Item1) / 2;
        //             xSum += weight * (data[i].Item2 + data[j].Item2) / 2;
        //             zSum += weight * timeDiff;
        //             totalWeight += weight;
        //         }
        //     }

        //     return (ySum / totalWeight, xSum / totalWeight, zSum / totalWeight);
        // }
    }
}
