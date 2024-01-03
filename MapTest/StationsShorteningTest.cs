using Map.Model;

using NUnit.Framework;

using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MapTest
{
    public class StationsShorteningTest
    {
        [TestCase]
        public void ShorteningTest()
        {
            var stations = Stations.Instance;
            var prop = stations.GetType().GetField("stations", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var stationMap = (Dictionary<string, Station>)(prop.GetValue(stations));

            var actual = stationMap.Values.Distinct().Select((station) =>
            {
                if (station.IsArea) { return new string[] { station.Name, station.Name }; }

                var match = StationNameShorter.ShortenPattern.Match(station.Name);
                if (match.Success)
                {
                    return new string[] { station.Name, match.Groups[1].Value };
                }
                return new string[] { station.Name, station.Name };
            }).Select((array) => string.Join(',', array));

            var directory = System.AppDomain.CurrentDomain.BaseDirectory;
            var filename = Path.Combine(directory, @"TestData/ShorteningTest.csv");

            // Note. expect を更新する場合
            // File.WriteAllLines(filename, actual, System.Text.Encoding.UTF8);

            var expect = File.ReadAllLines(filename, System.Text.Encoding.UTF8);
            CollectionAssert.AreEquivalent(expect, actual);
        }
    }
}
