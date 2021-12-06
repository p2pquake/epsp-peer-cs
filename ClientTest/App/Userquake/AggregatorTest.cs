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
                { "105", 10 },
                { "110", 1 },
                { "111", 2 },
            };
            var records = new (DateTime, string)[]
            {
                (DateTime.Today.AddSeconds(1000), "100"),
                (DateTime.Today.AddSeconds(1000), "100"),
                (DateTime.Today.AddSeconds(1015), "105"),
                (DateTime.Today.AddSeconds(1055), "100"),
                (DateTime.Today.AddSeconds(1096), "100"),
                (DateTime.Today.AddSeconds(1097), "105"),
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
            { "100", 100 },
            { "105", 200 },
            { "200", 300 },
            { "205", 10000 },
        };

        [TestCase]
        public void RaiseEventTest()
        {
            List<UserquakeEvaluateEventArgs> newEvents = new();
            List<UserquakeEvaluateEventArgs> updateEvents = new();

            aggregator.OnNew += (sender, e) => newEvents.Add(e);
            aggregator.OnUpdate += (sender, e) => updateEvents.Add(e);

            // Event 1
            (DateTime, string)[] userquakes = new[]
            {
                (DateTime.Today.AddSeconds(0), "100"),
                (DateTime.Today.AddSeconds(18), "200"),
                (DateTime.Today.AddSeconds(20), "100"),
                (DateTime.Today.AddSeconds(20), "100"),
                (DateTime.Today.AddSeconds(20), "100"),
            };
            foreach (var userquake in userquakes) { aggregator.AddUserquake(userquake.Item1, userquake.Item2, areaPeers); }
            Assert.That(newEvents, Is.Empty);
            Assert.That(updateEvents, Is.Empty);

            // Event 2
            userquakes = new[]
            {
                (DateTime.Today.AddSeconds(100), "100"),
                (DateTime.Today.AddSeconds(120), "200"),
                (DateTime.Today.AddSeconds(123), "100"),
                (DateTime.Today.AddSeconds(123), "100"),
                (DateTime.Today.AddSeconds(123), "100"),
                (DateTime.Today.AddSeconds(123), "100"),
                (DateTime.Today.AddSeconds(123), "100"),
            };
            foreach (var userquake in userquakes) { aggregator.AddUserquake(userquake.Item1, userquake.Item2, areaPeers); }

            Assert.That(newEvents, Has.Count.EqualTo(1));
            Assert.That(newEvents[0].Count, Is.EqualTo(7));
            Assert.That(newEvents[0].StartedAt, Is.EqualTo(DateTime.Today.AddSeconds(100)));
            Assert.That(newEvents[0].UpdatedAt, Is.EqualTo(DateTime.Today.AddSeconds(123)));
            Assert.That(newEvents[0].ConfidenceLevel, Is.EqualTo(3));
            Assert.That(newEvents[0].AreaConfidences, Has.Count.EqualTo(2));
            Assert.That(newEvents[0].AreaConfidences["100"].ConfidenceLevel, Is.EqualTo("E"));
            Assert.That(newEvents[0].AreaConfidences["100"].Count, Is.EqualTo(6));
            Assert.That(newEvents[0].AreaConfidences["200"].ConfidenceLevel, Is.EqualTo("E"));
            Assert.That(newEvents[0].AreaConfidences["200"].Count, Is.EqualTo(1));
            Assert.That(updateEvents, Is.Empty);

            aggregator.AddUserquake(DateTime.Today.AddSeconds(124), "100", areaPeers);
            aggregator.AddUserquake(DateTime.Today.AddSeconds(125), "100", areaPeers);

            Assert.That(newEvents, Has.Count.EqualTo(1));
            Assert.That(updateEvents, Has.Count.EqualTo(2));
            Assert.That(updateEvents.Last().Count, Is.EqualTo(9));
            Assert.That(updateEvents.Last().StartedAt, Is.EqualTo(newEvents[0].StartedAt));
            Assert.That(updateEvents.Last().UpdatedAt, Is.EqualTo(DateTime.Today.AddSeconds(125)));
            Assert.That(updateEvents.Last().Confidence, Is.GreaterThan(newEvents[0].Confidence));
            Assert.That(updateEvents.Last().AreaConfidences["100"].Confidence, Is.GreaterThan(newEvents[0].AreaConfidences["100"].Confidence));
            Assert.That(updateEvents.Last().AreaConfidences["100"].Count, Is.GreaterThan(newEvents[0].AreaConfidences["100"].Count));
            Assert.That(updateEvents.Last().AreaConfidences["200"].Confidence, Is.EqualTo(newEvents[0].AreaConfidences["200"].Confidence));
            Assert.That(updateEvents.Last().AreaConfidences["200"].Count, Is.EqualTo(newEvents[0].AreaConfidences["200"].Count));

            // Event 3
            userquakes = new[]
            {
                (DateTime.Today.AddSeconds(200), "100"),
                (DateTime.Today.AddSeconds(220), "200"),
                (DateTime.Today.AddSeconds(223), "100"),
                (DateTime.Today.AddSeconds(223), "100"),
                (DateTime.Today.AddSeconds(223), "100"),
                (DateTime.Today.AddSeconds(223), "100"),
                (DateTime.Today.AddSeconds(223), "100"),
            };
            foreach (var userquake in userquakes) { aggregator.AddUserquake(userquake.Item1, userquake.Item2, areaPeers); }
            Assert.That(newEvents, Has.Count.EqualTo(2));
            Assert.That(newEvents.Last().Count, Is.EqualTo(7));
            Assert.That(newEvents.Last().StartedAt, Is.EqualTo(DateTime.Today.AddSeconds(200)));
            Assert.That(newEvents.Last().UpdatedAt, Is.EqualTo(DateTime.Today.AddSeconds(223)));
        }

        [TestCase]
        public void EvaluateNotTrulyTest()
        {
            evaluateNotTruly(new[] {
                (DateTime.Today, "100")
            }, areaPeers);

            evaluateNotTruly(new[] {
                (DateTime.Today.AddSeconds(0.05), "100"),
                (DateTime.Today.AddSeconds(0.10), "100"),
            }, areaPeers);
        }

        [TestCase]
        public void EvaluateType1Test() { 
            evaluateNotTruly(new[] {
                (DateTime.Today.AddSeconds(0), "100"),
                (DateTime.Today.AddSeconds(18), "200"),
                (DateTime.Today.AddSeconds(20), "100"),
                (DateTime.Today.AddSeconds(20), "100"),
                (DateTime.Today.AddSeconds(20), "100"),
            }, areaPeers);

            evaluateTruly(new[] {
                (DateTime.Today.AddSeconds(0), "100"),
                (DateTime.Today.AddSeconds(20), "200"),
                (DateTime.Today.AddSeconds(24), "100"),
                (DateTime.Today.AddSeconds(24), "100"),
                (DateTime.Today.AddSeconds(24), "100"),
                (DateTime.Today.AddSeconds(24), "100"),
            }, areaPeers, 2);
        }

        [TestCase]
        public void EvaluateType2Test()
        {
            evaluateTruly(new[]
            {
                (DateTime.Today.AddSeconds(0), "100"),
                (DateTime.Today.AddSeconds(40), "100"),
                (DateTime.Today.AddSeconds(80), "100"),
                (DateTime.Today.AddSeconds(120), "100"),
                (DateTime.Today.AddSeconds(160), "100"),
            }.Concat(
                Enumerable.Repeat((DateTime.Today.AddSeconds(179), "100"), 24)
            ).Concat(
                new[]
                {
                    (DateTime.Today.AddSeconds(180), "200"),
                    (DateTime.Today.AddSeconds(180), "100"),
                }
            ).ToArray(), areaPeers, 2);

            evaluateTruly(new[]
            {
                (DateTime.Today.AddSeconds(0), "100"),
                (DateTime.Today.AddSeconds(40), "100"),
                (DateTime.Today.AddSeconds(80), "100"),
                (DateTime.Today.AddSeconds(120), "100"),
                (DateTime.Today.AddSeconds(160), "100"),
            }.Concat(
                Enumerable.Repeat((DateTime.Today.AddSeconds(179), "100"), 23)
            ).Concat(
                new[]
                {
                    (DateTime.Today.AddSeconds(180), "200"),
                    (DateTime.Today.AddSeconds(180), "100"),
                }
            ).ToArray(), areaPeers, 1);
        }

        [TestCase]
        public void EvaluateType3Test()
        {
            evaluateTruly(new[]
            {
                (DateTime.Today.AddSeconds(0), "100")
            }.Concat(
                Enumerable.Range(1, 94).Select(e => (DateTime.Today.AddSeconds(40*e), "205"))
            ).Concat(
                Enumerable.Range(95, 11).Select(e => (DateTime.Today.AddSeconds(40*e), "200"))
            ).ToArray(), areaPeers, 2);
            
            evaluateTruly(new[]
            {
                (DateTime.Today.AddSeconds(0), "100")
            }.Concat(
                Enumerable.Range(1, 94).Select(e => (DateTime.Today.AddSeconds(40*e), "205"))
            ).Concat(
                Enumerable.Range(95, 10).Select(e => (DateTime.Today.AddSeconds(40*e), "200"))
            ).ToArray(), areaPeers, 1);
        }

        [TestCase]
        public void EvaluateType4Test()
        {
            evaluateTruly(new[]
            {
                (DateTime.Today.AddSeconds(0), "200")
            }.Concat(
                Enumerable.Range(1, 11).Select(e => (DateTime.Today.AddSeconds(40*e), "200"))
            ).Concat(
                Enumerable.Range(12, 52).Select(e => (DateTime.Today.AddSeconds(40*e), "205"))
            ).ToArray(), areaPeers, 2);

            evaluateTruly(new[]
            {
                (DateTime.Today.AddSeconds(0), "200")
            }.Concat(
                Enumerable.Range(1, 10).Select(e => (DateTime.Today.AddSeconds(40*e), "200"))
            ).Concat(
                Enumerable.Range(11, 53).Select(e => (DateTime.Today.AddSeconds(40*e), "205"))
            ).ToArray(), areaPeers, 1);
        }

        [TestCase]
        public void EvaluateType5Test()
        {
            evaluateTruly(new[]
            {
                (DateTime.Today.AddSeconds(0), "100"),
                (DateTime.Today.AddSeconds(22), "100"),
                (DateTime.Today.AddSeconds(22), "100"),
                (DateTime.Today.AddSeconds(22), "100"),
            }, areaPeers, 2);

            evaluateNotTruly(new[]
            {
                (DateTime.Today.AddSeconds(0), "100"),
                (DateTime.Today.AddSeconds(22), "100"),
                (DateTime.Today.AddSeconds(22), "100"),
                (DateTime.Today.AddSeconds(22), "105"),
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

        [TestCase]
        public void AreaConfidenceTest()
        {
            var areaPeers = new Dictionary<string, int>()
            {
                { "010", 63 },
                { "015", 6 },
                { "025", 2 },
                { "030", 5 },
                { "035", 3 },
                { "040", 1 },
                { "045", 1 },
                { "050", 3 },
                { "055", 6 },
                { "065", 10 },
                { "070", 5 },
                { "075", 1 },
                { "100", 20 },
                { "105", 15 },
                { "106", 2 },
                { "110", 5 },
                { "111", 3 },
                { "115", 19 },
                { "120", 21 },
                { "125", 36 },
                { "130", 21 },
                { "135", 10 },
                { "140", 5 },
                { "141", 1 },
                { "142", 10 },
                { "143", 5 },
                { "150", 24 },
                { "151", 7 },
                { "152", 6 },
                { "200", 28 },
                { "205", 30 },
                { "210", 7 },
                { "215", 32 },
                { "220", 5 },
                { "225", 38 },
                { "230", 28 },
                { "231", 149 },
                { "232", 3 },
                { "240", 16 },
                { "241", 108 },
                { "242", 9 },
                { "250", 465 },
                { "270", 222 },
                { "275", 50 },
                { "300", 3 },
                { "301", 10 },
                { "302", 18 },
                { "305", 1 },
                { "310", 7 },
                { "315", 4 },
                { "320", 2 },
                { "325", 10 },
                { "330", 4 },
                { "340", 6 },
                { "345", 7 },
                { "350", 10 },
                { "351", 16 },
                { "355", 6 },
                { "400", 1 },
                { "405", 22 },
                { "410", 2 },
                { "411", 31 },
                { "415", 26 },
                { "416", 23 },
                { "420", 16 },
                { "425", 77 },
                { "430", 18 },
                { "435", 3 },
                { "440", 3 },
                { "445", 3 },
                { "455", 21 },
                { "460", 66 },
                { "465", 37 },
                { "470", 2 },
                { "475", 49 },
                { "480", 15 },
                { "490", 9 },
                { "495", 5 },
                { "500", 2 },
                { "505", 2 },
                { "510", 7 },
                { "520", 2 },
                { "525", 12 },
                { "530", 2 },
                { "535", 19 },
                { "541", 8 },
                { "545", 4 },
                { "550", 6 },
                { "555", 2 },
                { "560", 23 },
                { "570", 8 },
                { "575", 5 },
                { "576", 3 },
                { "581", 7 },
                { "600", 17 },
                { "601", 9 },
                { "602", 2 },
                { "605", 6 },
                { "610", 1 },
                { "615", 1 },
                { "620", 1 },
                { "625", 2 },
                { "641", 9 },
                { "646", 1 },
                { "650", 2 },
                { "651", 7 },
                { "655", 1 },
                { "656", 1 },
                { "660", 3 },
                { "665", 7 },
                { "670", 7 },
                { "675", 4 },
                { "701", 7 },
                { "710", 1 },
                { "900", 77 },
                { "901", 10 },
                { "905", 2 },
            };

            var userquakes = new (DateTime, string)[]
            {
			    (DateTime.Parse("2020/07/02 23:29:31.900"), "205"),
			    (DateTime.Parse("2020/07/02 23:29:35.961"), "215"),
			    (DateTime.Parse("2020/07/02 23:29:36.378"), "200"),
			    (DateTime.Parse("2020/07/02 23:29:37.989"), "231"),
			    (DateTime.Parse("2020/07/02 23:29:39.182"), "215"),
			    (DateTime.Parse("2020/07/02 23:29:42.570"), "241"),
			    (DateTime.Parse("2020/07/02 23:29:42.900"), "205"),
			    (DateTime.Parse("2020/07/02 23:29:43.512"), "200"),
			    (DateTime.Parse("2020/07/02 23:29:43.667"), "215"),
			    (DateTime.Parse("2020/07/02 23:29:43.719"), "240"),
			    (DateTime.Parse("2020/07/02 23:29:44.004"), "241"),
			    (DateTime.Parse("2020/07/02 23:29:44.466"), "231"),
			    (DateTime.Parse("2020/07/02 23:29:44.563"), "205"),
			    (DateTime.Parse("2020/07/02 23:29:45.352"), "241"),
			    (DateTime.Parse("2020/07/02 23:29:46.006"), "205"),
			    (DateTime.Parse("2020/07/02 23:29:46.706"), "241"),
			    (DateTime.Parse("2020/07/02 23:29:47.442"), "250"),
			    (DateTime.Parse("2020/07/02 23:29:47.522"), "250"),
			    (DateTime.Parse("2020/07/02 23:29:47.537"), "230"),
			    (DateTime.Parse("2020/07/02 23:29:48.233"), "250"),
			    (DateTime.Parse("2020/07/02 23:29:48.338"), "205"),
			    (DateTime.Parse("2020/07/02 23:29:48.465"), "231"),
			    (DateTime.Parse("2020/07/02 23:29:48.744"), "215"),
			    (DateTime.Parse("2020/07/02 23:29:49.021"), "701"),
			    (DateTime.Parse("2020/07/02 23:29:49.103"), "241"),
			    (DateTime.Parse("2020/07/02 23:29:49.153"), "230"),
			    (DateTime.Parse("2020/07/02 23:29:49.955"), "270"),
			    (DateTime.Parse("2020/07/02 23:29:51.255"), "151"),
			    (DateTime.Parse("2020/07/02 23:29:51.871"), "270"),
			    (DateTime.Parse("2020/07/02 23:29:52.070"), "270"),
			    (DateTime.Parse("2020/07/02 23:29:52.165"), "231"),
			    (DateTime.Parse("2020/07/02 23:29:53.130"), "241"),
			    (DateTime.Parse("2020/07/02 23:29:53.880"), "231"),
			    (DateTime.Parse("2020/07/02 23:29:56.442"), "250"),
			    (DateTime.Parse("2020/07/02 23:29:56.672"), "215"),
			    (DateTime.Parse("2020/07/02 23:29:57.253"), "240"),
			    (DateTime.Parse("2020/07/02 23:29:57.857"), "241"),
			    (DateTime.Parse("2020/07/02 23:29:58.707"), "250"),
			    (DateTime.Parse("2020/07/02 23:29:59.075"), "250"),
			    (DateTime.Parse("2020/07/02 23:29:59.181"), "241"),
			    (DateTime.Parse("2020/07/02 23:29:59.678"), "225"),
			    (DateTime.Parse("2020/07/02 23:30:04.106"), "250"),
			    (DateTime.Parse("2020/07/02 23:30:05.225"), "275"),
			    (DateTime.Parse("2020/07/02 23:30:07.287"), "250"),
			    (DateTime.Parse("2020/07/02 23:30:09.626"), "220"),
			    (DateTime.Parse("2020/07/02 23:30:11.352"), "220"),
			    (DateTime.Parse("2020/07/02 23:30:12.950"), "275"),
			    (DateTime.Parse("2020/07/02 23:30:13.663"), "250"),
			    (DateTime.Parse("2020/07/02 23:30:15.389"), "225"),
			    (DateTime.Parse("2020/07/02 23:30:21.496"), "150"),
			    (DateTime.Parse("2020/07/02 23:30:23.117"), "231"),
			    (DateTime.Parse("2020/07/02 23:30:29.460"), "410"),
			    (DateTime.Parse("2020/07/02 23:30:30.746"), "241"),
			    (DateTime.Parse("2020/07/02 23:30:34.520"), "231"),
			    (DateTime.Parse("2020/07/02 23:30:36.176"), "232"),
			    (DateTime.Parse("2020/07/02 23:30:39.519"), "270"),
			    (DateTime.Parse("2020/07/02 23:30:46.665"), "125"),
			    (DateTime.Parse("2020/07/02 23:30:51.433"), "241"),
			    (DateTime.Parse("2020/07/02 23:30:51.540"), "250"),
			    (DateTime.Parse("2020/07/02 23:30:55.627"), "241"),
			    (DateTime.Parse("2020/07/02 23:31:01.814"), "241"),
            };

            var expectedConfidenceAreas = new string[]
            {
                "220", "225", "230", "231", "232", "240", "241", "250", "150", "151", "200", "205", "215", "125", "270", "275", "410"
            };
            var expectedUnconfidenceAreas = new string[] { "701" };

            var eval = evaluate(userquakes, areaPeers);

            var confidenceAreas = eval.AreaConfidences.Where(e => e.Value.Confidence >= 0);
            var unconfidenceAreas = eval.AreaConfidences.Where(e => e.Value.Confidence < 0);

            Assert.That(confidenceAreas.Select(e => e.Key), Is.EquivalentTo(expectedConfidenceAreas));
            Assert.That(confidenceAreas.Select(e => e.Value.ConfidenceLevel), Is.All.EqualTo("E"));
            Assert.That(confidenceAreas.Select(e => e.Value.Count), Is.All.GreaterThan(0));

            Assert.That(unconfidenceAreas.Select(e => e.Key), Is.EquivalentTo(expectedUnconfidenceAreas));
            Assert.That(unconfidenceAreas.Select(e => e.Value.ConfidenceLevel), Is.All.EqualTo("F"));
            Assert.That(unconfidenceAreas.Select(e => e.Value.Count), Is.All.GreaterThan(0));
        }

        [TestCase]
        public void AreaConfidenceTest2()
        {
            var areaPeers = new Dictionary<string, int>()
            {
                { "900", 71 },
                { "420", 15 },
                { "250", 416 },
                { "310", 4 },
                { "460", 54 },
                { "205", 23 },
                { "405", 24 },
                { "475", 48 },
                { "241", 82 },
                { "115", 15 },
                { "580", 1 },
                { "560", 23 },
                { "570", 5 },
                { "215", 23 },
                { "231", 108 },
                { "010", 51 },
                { "065", 7 },
                { "150", 20 },
                { "411", 26 },
                { "120", 23 },
                { "430", 14 },
                { "270", 184 },
                { "200", 19 },
                { "240", 18 },
                { "305", 2 },
                { "541", 5 },
                { "225", 34 },
                { "415", 20 },
                { "465", 28 },
                { "143", 4 },
                { "535", 16 },
                { "355", 4 },
                { "675", 5 },
                { "425", 58 },
                { "581", 8 },
                { "320", 1 },
                { "040", 2 },
                { "142", 10 },
                { "125", 32 },
                { "480", 14 },
                { "230", 22 },
                { "525", 14 },
                { "242", 5 },
                { "100", 14 },
                { "655", 2 },
                { "610", 1 },
                { "140", 3 },
                { "330", 4 },
                { "302", 19 },
                { "340", 5 },
                { "325", 7 },
                { "135", 12 },
                { "105", 17 },
                { "050", 3 },
                { "275", 29 },
                { "490", 4 },
                { "110", 5 },
                { "455", 22 },
                { "130", 11 },
                { "615", 3 },
                { "605", 5 },
                { "301", 5 },
                { "350", 5 },
                { "545", 2 },
                { "550", 5 },
                { "141", 1 },
                { "300", 4 },
                { "665", 8 },
                { "901", 3 },
                { "470", 6 },
                { "600", 17 },
                { "905", 4 },
                { "670", 5 },
                { "015", 4 },
                { "315", 4 },
                { "445", 4 },
                { "510", 2 },
                { "416", 12 },
                { "030", 3 },
                { "152", 8 },
                { "651", 4 },
                { "515", 1 },
                { "345", 7 },
                { "151", 7 },
                { "650", 4 },
                { "265", 1 },
                { "055", 4 },
                { "701", 3 },
                { "505", 1 },
                { "500", 2 },
                { "220", 2 },
                { "351", 8 },
                { "070", 4 },
                { "210", 4 },
                { "641", 6 },
                { "410", 5 },
                { "601", 3 },
                { "530", 1 },
                { "111", 2 },
                { "520", 2 },
                { "575", 2 },
                { "025", 2 },
                { "656", 1 },
                { "435", 4 },
                { "075", 1 },
                { "646", 1 },
                { "440", 1 },
                { "576", 1 },
                { "045", 1 },
                { "625", 1 },
                { "495", 1 },
                { "400", 1 },
            };

            var userquakes = new (DateTime, string)[]
            {
			    (DateTime.Parse("2020/07/13 10:43:28.586"), "105"),
			    (DateTime.Parse("2020/07/13 10:43:43.067"), "105"),
			    (DateTime.Parse("2020/07/13 10:43:44.719"), "015"),
			    (DateTime.Parse("2020/07/13 10:43:57.093"), "111"),
			    (DateTime.Parse("2020/07/13 10:43:59.335"), "065"),
			    (DateTime.Parse("2020/07/13 10:44:04.450"), "015"),
			    (DateTime.Parse("2020/07/13 10:44:15.768"), "015"),
			    (DateTime.Parse("2020/07/13 10:44:18.584"), "100"),
			    (DateTime.Parse("2020/07/13 10:44:24.516"), "010"),
			    (DateTime.Parse("2020/07/13 10:44:37.494"), "065"),
			    (DateTime.Parse("2020/07/13 10:44:43.956"), "100"),
            };

            var expectedConfidenceEAreas = new string[] { "010", "100", "105", "111" };
            var expectedConfidenceDAreas = new string[] { "015" };
            var expectedUnconfidenceAreas = new string[] { "065" };

            var eval = evaluate(userquakes, areaPeers);

            var confidenceAreas = eval.AreaConfidences.Where(e => e.Value.Confidence >= 0);
            var unconfidenceAreas = eval.AreaConfidences.Where(e => e.Value.Confidence < 0);

            Assert.That(confidenceAreas.Select(e => e.Key), Is.EquivalentTo(expectedConfidenceEAreas.Concat(expectedConfidenceDAreas)));
            Assert.That(confidenceAreas.Select(e => e.Value.Count), Is.All.GreaterThan(0));
            Assert.That(confidenceAreas.Where(e => expectedConfidenceEAreas.Contains(e.Key)).Select(e => e.Value.ConfidenceLevel), Is.All.EqualTo("E"));
            Assert.That(confidenceAreas.Where(e => expectedConfidenceDAreas.Contains(e.Key)).Select(e => e.Value.ConfidenceLevel), Is.All.EqualTo("D"));

            Assert.That(unconfidenceAreas.Select(e => e.Key), Is.EquivalentTo(expectedUnconfidenceAreas));
            Assert.That(unconfidenceAreas.Select(e => e.Value.ConfidenceLevel), Is.All.EqualTo("F"));
            Assert.That(unconfidenceAreas.Select(e => e.Value.Count), Is.All.GreaterThan(0));
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
