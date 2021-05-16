using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Map.Model
{
    public record Station(string Prefecture, string Name, bool IsArea, bool IsJMA, double Latitude, double Longitude);

    public class Stations
    {
        public static Stations Instance { get; } = new();

        private readonly IReadOnlyDictionary<string, Station> stations;
        private readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, Station>> stationsByPrefecture;
        private readonly Regex cityNameRegex = new(@"^(?:余市町|田村市|玉村町|東村山市|武蔵村山市|羽村市|十日町市|上市町|大町市|名古屋中村区|大阪堺市.+?区|下市町|大村市|野々市市|四日市市|廿日市市|大町町|.+?[市区町村])", RegexOptions.Compiled);

        private Stations()
        {
            Dictionary<string, Station> sts = new();
            Dictionary<string, Dictionary<string, Station>> stsByPref = new();

            var areas = PointResource.Areas
                .Split('\n')
                .Skip(1)
                .Where((line) => line.Length > 0)
                .Select((line) => line.Split(','))
                .Select((items) => new Station(
                    items[0],
                    items[1],
                    true,
                    true,
                    double.Parse(items[2]),
                    double.Parse(items[3])
                )).ToList();
            areas.ForEach(area =>
            {
                sts[area.Name] = area;
                if (!stsByPref.ContainsKey(area.Prefecture))
                {
                    stsByPref[area.Prefecture] = new();
                }
                stsByPref[area.Prefecture][area.Name] = area;
            });

            var points = PointResource.Stations
                .Split('\n')
                .Skip(1)
                .Where((line) => line.Length > 0)
                .Select((line) => line.Split(','))
                .Select((items) => new Station(
                    items[0],
                    items[1],
                    false,
                    items[4] == "気象庁",
                    double.Parse(items[2]),
                    double.Parse(items[3])
                )).ToList();

            points.ForEach(point =>
            {
                sts[point.Name] = point;
                stsByPref[point.Prefecture][point.Name] = point;

                // 省略名
                if (cityNameRegex.Match(point.Name) is var m && m.Success)
                {
                    // 気象庁の震度観測点を優先
                    if (!sts.ContainsKey(m.Value) || !sts[m.Value].IsJMA)
                    {
                        sts[m.Value] = point;
                    }
                    if (!stsByPref[point.Prefecture].ContainsKey(m.Value) || !stsByPref[point.Prefecture][m.Value].IsJMA)
                    {
                        stsByPref[point.Prefecture][m.Value] = point;
                    }
                }
            });

            stations = sts;
            stationsByPrefecture = stsByPref.Select(e => new KeyValuePair<string, IReadOnlyDictionary<string, Station>>(e.Key, e.Value)).ToDictionary(e => e.Key, e => e.Value);
        }

        public GeoCoordinate GetArea(string name)
        {
            if (stations.ContainsKey(name) && stations[name].IsArea)
            {
                return new GeoCoordinate(stations[name].Latitude, stations[name].Longitude);
            }

            return null;
        }

        public GeoCoordinate GetPoint(string name, string pref = "")
        {
            var point = SearchPoint(name, pref);
            if (point != null)
            {
                return new GeoCoordinate(point.Latitude, point.Longitude);
            }

            return null;
        }

        private Station SearchPoint(string name, string pref = "")
        {
            // 都道府県 完全一致 → 都道府県 市区町村一致
            var target = stationsByPrefecture.ContainsKey(pref) ? stationsByPrefecture[pref] : stations;

            if (target.ContainsKey(name))
            {
                return target[name];
            }

            if (cityNameRegex.Match(name) is var m && m.Success && target.ContainsKey(m.Value))
            {
                return target[m.Value];
            }

            return null;
        }
    }
}
