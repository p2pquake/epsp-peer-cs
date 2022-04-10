using Map.Controller;
using Map.Model;

using NUnit.Framework;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;

namespace MapTest
{
    public class EarthquakeDrawTest
    {
        static readonly Func<Stream>[] drawActions = {
            // 2022/04/07 22:26
            () => new MapDrawer
            {
                Trim = true,
                MapType = MapType.JAPAN_1024,
                Hypocenter = new GeoCoordinate(29.4, 129.5),
                ObservationPoints = new List<ObservationPoint>
                {
                    new("é≠éôìáåß", "é≠éôìáè\ìáë∫", 10),
                },
            }.DrawAsPng(),
            // 2022/04/07 09:30
            () => new MapDrawer
            {
                Trim = true,
                MapType = MapType.JAPAN_2048,
                Hypocenter = new GeoCoordinate(34.9, 137.5),
            }.DrawAsPng(),

            // 2022/04/08 22:05
            () => new MapDrawer
            {
                MapType = MapType.JAPAN_8192,
                ObservationPoints = new List<ObservationPoint>
                {
                    new("", "êŒêÏåßî\ìo", 40),
                },
            }.DrawAsPng(),
        };

        [TestCaseSource(nameof(drawActions))]
        public void InternationalizationTest(Func<Stream> drawAction)
        {
            var drawResults = Internationalization.CultureNames.Select(cultureName =>
            {
                Internationalization.ReinstantiateSingletonInstances(cultureName);
                var ms = new MemoryStream();
                drawAction().CopyTo(ms);
                return ms.ToArray();
            }).ToList();

            drawResults.ForEach(drawResult =>
            {
                Assert.AreEqual(drawResults[0].Length, drawResult.Length);
                Assert.AreEqual(drawResults[0], drawResult);
            });
        }
    }
}