using Client.App.Userquake;

using NUnit.Framework;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientTest.App.Userquake
{
    [TestFixture]
    class AggregatorTest
    {
        Aggregator aggregator;

        [SetUp]
        public void SetUp()
        {
            aggregator = new();
        }

        [TestCase]
        public void AggregateTest()
        {
            var areaPeers = new Dictionary<string, int>()
            {
                { "100", 5 },
                { "101", 10 },
                { "110", 1 },
                { "111", 2 },
            };
            var records = new (DateTime, string)[]
            {
                (DateTime.Today.AddSeconds(1000), "100"),
                (DateTime.Today.AddSeconds(1000), "100"),
                (DateTime.Today.AddSeconds(1015), "101"),
                (DateTime.Today.AddSeconds(1055), "100"),
                (DateTime.Today.AddSeconds(1096), "100"),
                (DateTime.Today.AddSeconds(1097), "101"),
                (DateTime.Today.AddSeconds(1150), "110"),
            };

            var userquakes = new Dictionary<DateTime, IUserquakeEvaluation>();

            foreach (var record in records)
            {
                aggregator.AddUserquake(record.Item1, record.Item2, areaPeers);
                userquakes[aggregator.Evaluation.StartedAt] = aggregator.Evaluation;
            }

            Assert.AreEqual(3, userquakes.Count);

            Assert.Contains(DateTime.Today.AddSeconds(1000), userquakes.Keys);
            var e1 = userquakes[DateTime.Today.AddSeconds(1000)];
            Assert.AreEqual(4, e1.Count);

            Assert.Contains(DateTime.Today.AddSeconds(1096), userquakes.Keys);
            var e2 = userquakes[DateTime.Today.AddSeconds(1096)];
            Assert.AreEqual(2, e2.Count);

            Assert.Contains(DateTime.Today.AddSeconds(1150), userquakes.Keys);
            var e3 = userquakes[DateTime.Today.AddSeconds(1150)];
            Assert.AreEqual(1, e3.Count);
        }
    }
}
