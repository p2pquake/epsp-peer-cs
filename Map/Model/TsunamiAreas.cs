using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Map.Model
{
    public record TsunamiArea(string Name, GeoCoordinate[][] Coordinates);

    public class TsunamiAreas
    {
        public static TsunamiAreas Instance { get; } = new();

        private readonly IReadOnlyDictionary<string, TsunamiArea> tsunamiAreas;

        private TsunamiAreas()
        {
            Dictionary<string, TsunamiArea> areas = new();

            var jsons = JsonSerializer.Deserialize<MultiLineGeoJson>(PointResource.TsunamiAreaGeoJsons, new JsonSerializerOptions { IncludeFields = true });
            var areaCodes = PointResource.TsunamiAreaCodes.Replace("\r", "").Split('\n').Where((line) => line.Length > 0).Select((line) => line.Split(',')).ToDictionary(e => e[0], e => e[1]);

            foreach (var feature in jsons.features)
            {
                var name = areaCodes[feature.properties.code];
                var area = new TsunamiArea(
                    name,
                    feature.geometry.coordinates
                    .Select(e => e.Select(e => new GeoCoordinate(e[1], e[0])).ToArray()).ToArray()
                );

                // 既に存在してたら足す
                if (areas.ContainsKey(name))
                {
                    areas[name] = new TsunamiArea(
                        name,
                        areas[name].Coordinates.Concat(area.Coordinates).ToArray()
                        );
                }
                else
                {
                    areas.Add(name, area);
                }

            }

            tsunamiAreas = areas;
        }

        public TsunamiArea GetArea(string name)
        {
            if (!tsunamiAreas.ContainsKey(name))
            {
                return null;
            }

            return tsunamiAreas[name];
        }
    }
}
