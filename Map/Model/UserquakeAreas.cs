using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Map.Model
{
    public record UserquakeArea(string AreaCode, string Region, string Prefecture, string Area, double Latitude, double Longitude);

    public class UserquakeAreas
    {
        public static UserquakeAreas Instance { get; private set; } = new();

        private readonly IReadOnlyDictionary<string, UserquakeArea> areas;
        private readonly IReadOnlyDictionary<string, GeoCoordinate[][]> areaGeoCoordinates;

        private UserquakeAreas()
        {
            areas = PointResource.UserquakeAreas
                .Split('\n')
                .Skip(1)
                .Where((line) => line.Length > 0)
                .Select((line) => line.Split(','))
                .ToDictionary(
                    items => items[0],
                    items => new UserquakeArea(
                        items[0],
                        items[1],
                        items[2],
                        items[3],
                        double.Parse(items[4], NumberFormatInfo.InvariantInfo),
                        double.Parse(items[5], NumberFormatInfo.InvariantInfo)
                        )
                );

            // 地震感知情報の地域コード → 地震情報 GeoJSON のコード(複数)
            var uqAreaCodes2EqAreaCodes = PointResource.UserquakeAreas2EarthquakeAreaCodes.Replace("\r", "").Split('\n').Skip(1).Where((line) => line.Length > 0).Select((line) => line.Split(',')).ToDictionary(e => e[0], e => e.Skip(1));

            // GeoJSON を読み込み、 コード -> 多角形データ の形にしておく
            var areaGeoJsons = JsonSerializer.Deserialize<GeoJson>(PointResource.AreaGeoJsons, new JsonSerializerOptions { IncludeFields = true });
            var userquakeAreaGeoJsons = JsonSerializer.Deserialize<GeoJson>(PointResource.UserquakeAreaGeoJsons, new JsonSerializerOptions { IncludeFields = true });

            var geojsonCode2GeoCoordinates = new Dictionary<string, GeoCoordinate[][]>();
            SetGeoJsonMap(areaGeoJsons.features, geojsonCode2GeoCoordinates);
            SetGeoJsonMap(userquakeAreaGeoJsons.features, geojsonCode2GeoCoordinates);

            // 地震感知情報の地域コード -> 多角形データ の形にもってく
            areaGeoCoordinates = uqAreaCodes2EqAreaCodes.ToDictionary(uq2eq => uq2eq.Key,
                uq2eq => uq2eq.Value.Select(code => geojsonCode2GeoCoordinates[code]).Aggregate((acc, item) => acc.Concat(item).ToArray()));
        }

        private void SetGeoJsonMap(Feature[] geoJsonFeatures, Dictionary<string, GeoCoordinate[][]> destination) 
        {
            foreach (var feature in geoJsonFeatures)
            {
                var code = feature.properties.code;
                var geoCoordinates = feature.geometry.coordinates.Select(e => e[0].Select(e => new GeoCoordinate(e[1], e[0])).ToArray()).ToArray();

                if (destination.ContainsKey(code))
                {
                    destination[code] = destination[code].Concat(geoCoordinates).ToArray();
                } else
                {
                    destination[code] = geoCoordinates;
                }
            }
        }

        public bool ContainsKey(string areaCode)
        {
            return areas.ContainsKey(areaCode);
        }

        public GeoCoordinate Get(string areaCode)
        {
            return new GeoCoordinate(areas[areaCode].Latitude, areas[areaCode].Longitude);
        }

        // XXX: ContainsKey と役割があいまい。
        public bool ContainsMultiPolygonKey(string areaCode)
        {
            return areaGeoCoordinates.ContainsKey(areaCode);
        }

        public GeoCoordinate[][] GetMultiPolygon(string areaCode)
        {
            return areaGeoCoordinates[areaCode];
        }
    }
}
