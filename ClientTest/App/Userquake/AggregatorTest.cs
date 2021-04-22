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

        private readonly Dictionary<string, int> areaPeers = new()
        {
            { "101", 100 },
            { "102", 200 },
            { "201", 300 },
            { "202", 10000 },
        };

        [TestCase]
        public void EvaluateNotTrulyTest()
        {
            evaluateNotTruly(new[] {
                (DateTime.Today, "101")
            }, areaPeers);

            evaluateNotTruly(new[] {
                (DateTime.Today.AddSeconds(0.05), "101"),
                (DateTime.Today.AddSeconds(0.10), "101"),
            }, areaPeers);
        }

        [TestCase]
        public void EvaluateType1Test() { 
            evaluateNotTruly(new[] {
                (DateTime.Today.AddSeconds(0), "101"),
                (DateTime.Today.AddSeconds(18), "201"),
                (DateTime.Today.AddSeconds(20), "101"),
                (DateTime.Today.AddSeconds(20), "101"),
                (DateTime.Today.AddSeconds(20), "101"),
            }, areaPeers);

            evaluateTruly(new[] {
                (DateTime.Today.AddSeconds(0), "101"),
                (DateTime.Today.AddSeconds(20), "201"),
                (DateTime.Today.AddSeconds(24), "101"),
                (DateTime.Today.AddSeconds(24), "101"),
                (DateTime.Today.AddSeconds(24), "101"),
                (DateTime.Today.AddSeconds(24), "101"),
            }, areaPeers, 2);
        }

        [TestCase]
        public void EvaluateType2Test()
        {
            evaluateTruly(new[]
            {
                (DateTime.Today.AddSeconds(0), "101"),
                (DateTime.Today.AddSeconds(40), "101"),
                (DateTime.Today.AddSeconds(80), "101"),
                (DateTime.Today.AddSeconds(120), "101"),
                (DateTime.Today.AddSeconds(160), "101"),
            }.Concat(
                Enumerable.Repeat((DateTime.Today.AddSeconds(179), "101"), 24)
            ).Concat(
                new[]
                {
                    (DateTime.Today.AddSeconds(180), "201"),
                    (DateTime.Today.AddSeconds(180), "101"),
                }
            ).ToArray(), areaPeers, 2);

            evaluateTruly(new[]
            {
                (DateTime.Today.AddSeconds(0), "101"),
                (DateTime.Today.AddSeconds(40), "101"),
                (DateTime.Today.AddSeconds(80), "101"),
                (DateTime.Today.AddSeconds(120), "101"),
                (DateTime.Today.AddSeconds(160), "101"),
            }.Concat(
                Enumerable.Repeat((DateTime.Today.AddSeconds(179), "101"), 23)
            ).Concat(
                new[]
                {
                    (DateTime.Today.AddSeconds(180), "201"),
                    (DateTime.Today.AddSeconds(180), "101"),
                }
            ).ToArray(), areaPeers, 1);
        }

        [TestCase]
        public void EvaluateType3Test()
        {
            evaluateTruly(new[]
            {
                (DateTime.Today.AddSeconds(0), "101")
            }.Concat(
                Enumerable.Range(1, 94).Select(e => (DateTime.Today.AddSeconds(40*e), "202"))
            ).Concat(
                Enumerable.Range(95, 11).Select(e => (DateTime.Today.AddSeconds(40*e), "201"))
            ).ToArray(), areaPeers, 2);
            
            evaluateTruly(new[]
            {
                (DateTime.Today.AddSeconds(0), "101")
            }.Concat(
                Enumerable.Range(1, 94).Select(e => (DateTime.Today.AddSeconds(40*e), "202"))
            ).Concat(
                Enumerable.Range(95, 10).Select(e => (DateTime.Today.AddSeconds(40*e), "201"))
            ).ToArray(), areaPeers, 1);
        }

        [TestCase]
        public void EvaluateType4Test()
        {
            evaluateTruly(new[]
            {
                (DateTime.Today.AddSeconds(0), "201")
            }.Concat(
                Enumerable.Range(1, 11).Select(e => (DateTime.Today.AddSeconds(40*e), "201"))
            ).Concat(
                Enumerable.Range(12, 52).Select(e => (DateTime.Today.AddSeconds(40*e), "202"))
            ).ToArray(), areaPeers, 2);

            evaluateTruly(new[]
            {
                (DateTime.Today.AddSeconds(0), "201")
            }.Concat(
                Enumerable.Range(1, 10).Select(e => (DateTime.Today.AddSeconds(40*e), "201"))
            ).Concat(
                Enumerable.Range(11, 53).Select(e => (DateTime.Today.AddSeconds(40*e), "202"))
            ).ToArray(), areaPeers, 1);
        }

        [TestCase]
        public void EvaluateType5Test()
        {
            evaluateTruly(new[]
            {
                (DateTime.Today.AddSeconds(0), "101"),
                (DateTime.Today.AddSeconds(22), "101"),
                (DateTime.Today.AddSeconds(22), "101"),
                (DateTime.Today.AddSeconds(22), "101"),
            }, areaPeers, 2);

            evaluateNotTruly(new[]
            {
                (DateTime.Today.AddSeconds(0), "101"),
                (DateTime.Today.AddSeconds(22), "101"),
                (DateTime.Today.AddSeconds(22), "101"),
                (DateTime.Today.AddSeconds(22), "102"),
            }, areaPeers);
        }

        private void evaluateNotTruly((DateTime, string)[] userquakes, IReadOnlyDictionary<string, int> areaPeers)
        {
            var e = evaluate(userquakes, areaPeers);
            Assert.AreEqual(0, e.ConfidenceLevel);
        }

        private void evaluateTruly((DateTime, string)[] userquakes, IReadOnlyDictionary<string, int> areaPeers, int expectedLevel)
        {
            var e = evaluate(userquakes, areaPeers);
            Assert.AreEqual(expectedLevel, e.ConfidenceLevel);
        }

        private IUserquakeEvaluation evaluate((DateTime, string)[] userquakes, IReadOnlyDictionary<string, int> areaPeers)  
        {
            aggregator = new Aggregator();
            foreach (var userquake in userquakes)
            {
                aggregator.AddUserquake(userquake.Item1, userquake.Item2, areaPeers);
            }

            return aggregator.Evaluation;
        }
    }
}
