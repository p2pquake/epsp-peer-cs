﻿using Client.App.Userquake;

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
    class AggregatorMultithreadTest
    {
        Aggregator aggregator;

        [SetUp]
        public void SetUp()
        {
            aggregator = new();
        }

        [TestCase]
        public void MultithreadTest()
        {
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
                var r = new Random();
                var sw = new Stopwatch();
                sw.Start();
                while (sw.ElapsedMilliseconds < 5000)
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
    }
}
