using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Map.Model
{
    public record ObservationArea(string Prefecture, string Name, GeoCoordinate[][] Coordinates);

    public class ObservationAreas
    {
        public static ObservationAreas Instance { get; private set; } = new();

        private readonly IReadOnlyDictionary<string, ObservationArea> observationAreas;

        private ObservationAreas()
        {
            Dictionary<string, ObservationArea> areas = new();

            var jsons = JsonSerializer.Deserialize<GeoJson>(PointResource.AreaGeoJsons, new JsonSerializerOptions { IncludeFields = true });
            var areaCodes = PointResource.EarthquakeAreaCodes.Replace("\r", "").Split('\n').Where((line) => line.Length > 0).Select((line) => line.Split(',')).ToDictionary(e => e[0], e => e[1]);

            foreach (var feature in jsons.features)
            {
                var name = areaCodes[feature.properties.code];
                var area = new ObservationArea(
                    null,
                    name,
                    feature.geometry.coordinates
                    .Select(e => e[0].Select(e => new GeoCoordinate(e[1], e[0])).ToArray()).ToArray()
                    );

                // 既に存在してたら足す
                if (areas.ContainsKey(name))
                {
                    areas[name] = new ObservationArea(
                        null,
                        name,
                        areas[name].Coordinates.Concat(area.Coordinates).ToArray()
                        );
                } else
                {
                    areas.Add(name, area);
                }

            }

            observationAreas = areas;
        }

        public ObservationArea GetArea(string name)
        {
            if (!observationAreas.ContainsKey(name))
            {
                return null;
            }

            return observationAreas[name];
        }
    }
}
