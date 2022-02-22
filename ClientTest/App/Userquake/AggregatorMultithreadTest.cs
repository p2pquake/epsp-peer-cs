using Client.App.Userquake;

using NUnit.Framework;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClientTest.App.Userquake
{
    [TestFixture]
    [Parallelizable(scope: ParallelScope.All)]
    class AggregatorMultithreadTest
    {
        [TestCase]
        [Repeat(3)]
        public void MultithreadTest()
        {
            var aggregator = new Aggregator();

            // ピア分布図データを用意
            var areaPeers = new Dictionary<string, int>()
            {
                { "010", 10 },
                { "015", 10 },
            };

            var count = 0;

            Action addUserquakes = () =>
            {
                var actionCount = 0;
                var sw = new Stopwatch();
                sw.Start();
                while (sw.ElapsedMilliseconds < 2000)
                {
                    aggregator.AddUserquake(DateTime.Now, "010", areaPeers);
                    aggregator.AddUserquake(DateTime.Now, "015", areaPeers);
                    actionCount += 2;
                }
                Interlocked.Add(ref count, actionCount);
            };

            // マルチスレッドで地震感知情報のイベントを発火
            Parallel.Invoke(Enumerable.Repeat(addUserquakes, 10).ToArray());

            Assert.AreEqual(count, aggregator.Evaluation.Count);
            Console.WriteLine($"Count: {count}");
        }

        [TestCase]
        [Repeat(3)]
        public void EventOrderTest()
        {
            var aggregator = new Aggregator();

            // ピア分布図データを用意
            var areaPeers = new Dictionary<string, int>()
            {
                { "010", 10 },
                { "015", 10 },
            };

            var newEventOccurred = 0;
            aggregator.OnNew += (s, e) =>
            {
                if (1 == Interlocked.Exchange(ref newEventOccurred, 1))
                {
                    Assert.Fail("Duplicate OnNew event occurred");
                }
            };
            aggregator.OnUpdate += (s, e) =>
            {
                if (0 == Interlocked.Exchange(ref newEventOccurred, 1))
                {
                    Assert.Fail("OnUpdate event occurred before OnNew event");
                }
            };

            Action addUserquakes = () =>
            {
                var sw = new Stopwatch();
                sw.Start();
                while (sw.ElapsedMilliseconds < 2000)
                {
                    aggregator.AddUserquake(DateTime.Now, "010", areaPeers);
                    aggregator.AddUserquake(DateTime.Now, "015", areaPeers);
                }
            };

            // マルチスレッドで地震感知情報のイベントを発火
            Parallel.Invoke(Enumerable.Repeat(addUserquakes, 10).ToArray());
        }
    }
}
