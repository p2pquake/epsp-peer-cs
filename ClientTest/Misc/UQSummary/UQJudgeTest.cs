using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.Misc.UQSummary;
using NUnit.Framework;

namespace ClientTest.Misc.UQSummary
{
    [TestFixture]
    class UQJudgeTest
    {
        private IUQManager uqManager;
        private IUQJudge uqJudge;
        private DateTime protocolTime;

        [SetUp]
        public void SetUp()
        {
            uqManager = new UQManager();
            uqJudge = new SimpleUQJudge();
            uqManager.ProtocolTime = GetProtocolTime;
            uqManager.UQJudge = uqJudge;
        }

        [Test]
        public void IsOnGoingTest()
        {
            Assert.IsFalse(uqManager.IsOnGoing);

            protocolTime = DateTime.Parse("2017/12/31 23:59:30");
            uqManager.Add("100");
            Assert.IsFalse(uqManager.IsOnGoing);

            protocolTime = DateTime.Parse("2018/01/01 00:00:00");
            uqManager.Add("101");
            Assert.IsFalse(uqManager.IsOnGoing);

            protocolTime = DateTime.Parse("2018/01/01 00:00:29");
            uqManager.Add("102");
            Assert.IsFalse(uqManager.IsOnGoing);

            protocolTime = DateTime.Parse("2018/01/01 00:00:58");
            uqManager.Add("102");
            Assert.IsTrue(uqManager.IsOnGoing);

            protocolTime = DateTime.Parse("2018/01/01 00:01:27");
            uqManager.Add("103");
            Assert.IsTrue(uqManager.IsOnGoing);

            protocolTime = DateTime.Parse("2018/01/01 00:01:57");
            uqManager.Add("104");
            Assert.IsFalse(uqManager.IsOnGoing);
        }

        [Test]
        public void GetCurrentSummaryTest()
        {
            Assert.IsEmpty(uqManager.GetCurrentSummary());

            protocolTime = DateTime.Parse("2017/12/31 23:59:30");
            uqManager.Add("100");
            CollectionAssert.AreEqual(
                new Dictionary<string, int> { { "100", 1 } },
                uqManager.GetCurrentSummary()
                );

            protocolTime = DateTime.Parse("2018/01/01 00:00:00");
            uqManager.Add("101");
            CollectionAssert.AreEqual(
                new Dictionary<string, int> { { "101", 1 } },
                uqManager.GetCurrentSummary()
                );

            protocolTime = DateTime.Parse("2018/01/01 00:00:29");
            uqManager.Add("102");
            CollectionAssert.AreEqual(
                new Dictionary<string, int> { { "101", 1 }, { "102", 1 } },
                uqManager.GetCurrentSummary()
                );

            protocolTime = DateTime.Parse("2018/01/01 00:00:58");
            uqManager.Add("102");
            CollectionAssert.AreEqual(
                new Dictionary<string, int> { { "101", 1 }, { "102", 2 } },
                uqManager.GetCurrentSummary()
                );

            protocolTime = DateTime.Parse("2018/01/01 00:01:27");
            uqManager.Add("103");
            CollectionAssert.AreEqual(
                new Dictionary<string, int> { { "101", 1 }, { "102", 2 }, { "103", 1 } },
                uqManager.GetCurrentSummary()
                );

            protocolTime = DateTime.Parse("2018/01/01 00:01:57");
            uqManager.Add("104");
            CollectionAssert.AreEqual(
                new Dictionary<string, int> { { "104", 1 } },
                uqManager.GetCurrentSummary()
                );
        }

        [Test]
        public void EventTest()
        {
            bool occurred = false;
            bool updated = false;

            uqManager.Occurred += (s, e) => { occurred = true; };
            uqManager.Updated += (s, e) => { updated = true; };

            protocolTime = DateTime.Parse("2017/12/31 23:59:30");
            uqManager.Add("100");
            Assert.IsFalse(occurred);
            Assert.IsFalse(updated);

            protocolTime = DateTime.Parse("2018/01/01 00:00:00");
            uqManager.Add("101");
            Assert.IsFalse(occurred);
            Assert.IsFalse(updated);

            protocolTime = DateTime.Parse("2018/01/01 00:00:29");
            uqManager.Add("102");
            Assert.IsFalse(occurred);
            Assert.IsFalse(updated);

            protocolTime = DateTime.Parse("2018/01/01 00:00:58");
            uqManager.Add("102");
            Assert.IsTrue(occurred);
            Assert.IsFalse(updated);

            occurred = false;

            protocolTime = DateTime.Parse("2018/01/01 00:01:27");
            uqManager.Add("103");
            Assert.IsFalse(occurred);
            Assert.IsTrue(updated);

            updated = false;

            protocolTime = DateTime.Parse("2018/01/01 00:01:57");
            uqManager.Add("104");
            Assert.IsFalse(occurred);
            Assert.IsFalse(occurred);
        }
        

        private DateTime GetProtocolTime()
        {
            return protocolTime;
        }
    }
}
