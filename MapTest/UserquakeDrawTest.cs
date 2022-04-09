using Map.Controller;
using Map.Model;

using NUnit.Framework;

using System;
using System.Collections.Generic;
using System.Threading;

namespace MapTest
{
    public class UserquakeDrawTest
    {
        [TestCaseSource(typeof(Internationalization), nameof(Internationalization.CultureNames))]
        public void InternationalizationTest(string cultureName)
        {
            Internationalization.ReinstantiateSingletonInstances(cultureName);

            new MapDrawer
            {
                MapType = MapType.JAPAN_2048,
                UserquakePoints = new List<UserquakePoint>
                {
                    new UserquakePoint("010", 0.5),
                    new UserquakePoint("700", 0.5),
                },
            }.DrawAsPng();
        }
    }
}