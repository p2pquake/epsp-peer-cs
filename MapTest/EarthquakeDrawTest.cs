using Map.Controller;
using Map.Model;

using NUnit.Framework;

using System;
using System.Collections.Generic;
using System.Threading;

namespace MapTest
{
    public class EarthquakeDrawTest
    {

        [TestCaseSource(typeof(Internationalization), nameof(Internationalization.CultureNames))]
        public void InternationalizationTest(string cultureName)
        {
            Internationalization.ReinstantiateSingletonInstances(cultureName);

            // 2022/04/07 22:26
            new MapDrawer
            {
                Trim = true,
                MapType = MapType.JAPAN_1024,
                Hypocenter = new GeoCoordinate(29.4, 129.5),
                ObservationPoints = new List<ObservationPoint>
                {
                    new("é≠éôìáåß", "é≠éôìáè\ìáë∫", 10),
                },
            }.DrawAsPng();

            // 2022/04/07 09:30
            new MapDrawer
            {
                Trim = true,
                MapType = MapType.JAPAN_2048,
                Hypocenter = new GeoCoordinate(34.9, 137.5),
            }.DrawAsPng();

            // 2022/04/08 22:05
            new MapDrawer
            {
                MapType = MapType.JAPAN_8192,
                ObservationPoints = new List<ObservationPoint>
                {
                    new("", "êŒêÏåßî\ìo", 40),
                },
            }.DrawAsPng();
        }
    }
}