using NUnit.Framework;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ClientTest.App.Userquake
{
    [TestFixture]
    class UserquakeEvaluateEventArgsTest
    {
        [TestCase]
        public void EvaluationConfidenceLevelMapTest()
        {
            var patterns = new KeyValuePair<double, int>[]
            {
                new KeyValuePair<double, int>(0.0, 0),
                new KeyValuePair<double, int>(0.97015, 1),
                new KeyValuePair<double, int>(0.96774, 2),
                new KeyValuePair<double, int>(0.97024, 3),
                new KeyValuePair<double, int>(0.98052, 4),
            };

            foreach (var pattern in patterns) {
                var e = new Client.App.Userquake.UserquakeEvaluateEventArgs()
                {
                    StartedAt = DateTime.MinValue,
                    UpdatedAt = DateTime.MinValue,
                    Count = 0,
                    Confidence = pattern.Key,
                    AreaConfidences = null,
                };

                Assert.AreEqual(pattern.Value, e.ConfidenceLevel);
            }
        }

        [TestCase]
        public void AreaConfidenceLevelMapTest()
        {
            var patterns = new KeyValuePair<double, string>[]
            {
                new KeyValuePair<double, string>(-0.01, "F"),
                new KeyValuePair<double, string>(0.0, "E"),
                new KeyValuePair<double, string>(0.1999999, "E"),
                new KeyValuePair<double, string>(0.2, "D"),
                new KeyValuePair<double, string>(0.3999999, "D"),
                new KeyValuePair<double, string>(0.4, "C"),
                new KeyValuePair<double, string>(0.5999999, "C"),
                new KeyValuePair<double, string>(0.6, "B"),
                new KeyValuePair<double, string>(0.7999999, "B"),
                new KeyValuePair<double, string>(0.8, "A"),
            };

            foreach (var pattern in patterns)
            {
                var e = new Client.App.Userquake.UserquakeEvaluationArea();
                var prop = e.GetType().GetProperty("Confidence", BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.Instance);
                prop.SetValue(e, pattern.Key);

                Assert.AreEqual(pattern.Value, e.ConfidenceLevel);
            }
        }
    }
}
