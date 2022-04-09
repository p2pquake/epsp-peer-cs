using Map.Controller;
using Map.Model;

using NUnit.Framework;

using System;
using System.Collections.Generic;
using System.Threading;

namespace MapTest
{
    public class TsunamiDrawTest
    {
        [TestCaseSource(typeof(Internationalization), nameof(Internationalization.CultureNames))]
        public void InternationalizationTest(string cultureName)
        {
            Internationalization.ReinstantiateSingletonInstances(cultureName);

            // 2022/03/16 23:39
            new MapDrawer
            {
                MapType = MapType.JAPAN_2048,
                TsunamiPoints = new List<TsunamiPoint>
                {
                    new TsunamiPoint("ã{èÈåß", TsunamiCategory.Advisory),
                    new TsunamiPoint("ïüìáåß", TsunamiCategory.Advisory),
                },
            }.DrawAsPng();
        }
    }
}