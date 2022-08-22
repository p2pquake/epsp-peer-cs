using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Map.Model
{
    public class EEWAreas
    {
        public static EEWAreas Instance { get; private set; } = new();

        private readonly IReadOnlyDictionary<string, GeoCoordinate[][]> areaGeoCoordinates;
        private readonly IReadOnlyDictionary<string, string[]> prefs2eqAreaCodes;

        private EEWAreas()
        {
            // 緊急地震速報（警報）の府県予報区地域コード → 地震情報 GeoJSON のコード(複数)
            prefs2eqAreaCodes = PointResource.EEWPrefs2EarthquakeAreaCodes.Replace("\r", "").Split('\n').Skip(1).Where((line) => line.Length > 0).Select((line) => line.Split(',')).ToDictionary(e => e[0], e => e.Skip(1).ToArray());

            // GeoJSON を読み込み、 コード -> 多角形データ の形にしておく
            var areaGeoJsons = JsonSerializer.Deserialize<GeoJson>(PointResource.AreaGeoJsons, new JsonSerializerOptions { IncludeFields = true });

            var geojsonCode2GeoCoordinates = new Dictionary<string, GeoCoordinate[][]>();
            SetGeoJsonMap(areaGeoJsons.features, geojsonCode2GeoCoordinates);

            // 緊急地震速報（警報）の府県予報区地域コード -> 多角形データ の形にもってく
            areaGeoCoordinates = prefs2eqAreaCodes.ToDictionary(uq2eq => uq2eq.Key,
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
            return areaGeoCoordinates.ContainsKey(areaCode);
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

        public string[] GetEQAreaCodes(string areaCode)
        {
            return prefs2eqAreaCodes[areaCode];
        }
    }
}
