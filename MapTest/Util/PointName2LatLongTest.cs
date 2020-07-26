using Map.Map;
using Map.Util;

using NUnit.Framework;
using System;

namespace Map.UtilTest
{
    [TestFixture]
    public class PointName2LatLongTest
    {
        private static readonly double delta = 0.01;

        [TestCase]
        public void Convert001()
        {
            // ref. https://www.kyoshin.bosai.go.jp/kyoshin/db/index.html
            double[] point = PointName2LatLong.convert("稚内市宗谷岬");
            areEqualWithin(new double[] { 45.5132, 141.9548 }, point, delta);
        }

        [TestCase]
        public void ConvertNotExists()
        {
            double[] point = PointName2LatLong.convert("存在しない観測点名称");
            Assert.IsNull(point);
        }

        private void areEqualWithin(double[] expected, double[] actual, double delta)
        {
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], actual[i], delta);
            }
        }
    }
}
