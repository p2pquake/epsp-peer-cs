using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Map.Model
{
    public record UserquakeArea(string AreaCode, string Region, string Prefecture, string Area, double Latitude, double Longitude);

    public class UserquakeAreas
    {
        public static UserquakeAreas Instance { get; } = new();

        private readonly IReadOnlyDictionary<string, UserquakeArea> areas;

        private UserquakeAreas()
        {
            areas = PointResource.UserquakeAreas
                .Split('\n')
                .Skip(1)
                .Where((line) => line.Length > 0)
                .Select((line) => line.Split(','))
                .ToDictionary(
                    items => items[0],
                    items => new UserquakeArea(items[0], items[1], items[2], items[3], double.Parse(items[4]), double.Parse(items[5]))
                );
        }

        public bool ContainsKey(string AreaCode)
        {
            return areas.ContainsKey(AreaCode);
        }

        public GeoCoordinate Get(string AreaCode)
        {
            return new GeoCoordinate(areas[AreaCode].Latitude, areas[AreaCode].Longitude);
        }
    }
}
