using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Client.Common.Net;
using NUnit.Framework;

namespace ClientTest.Peer.Manager
{
    [TestFixture]
    class PeerReadLineTest
    {
        Client.Peer.Manager.PeerManager peerManager;
        Client.Peer.Manager.Peer peer;

        [SetUp]
        public void CreatePeer()
        {
            peerManager = new Client.Peer.Manager.PeerManager();
            peerManager.ProtocolTime = () => { return DateTime.Now; };
            peerManager.PeerCount = () => { return 1000; };
            peerManager.PeerId = () => { return 1; };
            peerManager.OnAreapeers += (s, e) => { };
            peerManager.OnEarthquake += (s, e) => { };
            peerManager.OnEEWTest += (s, e) => { };
            peerManager.OnTsunami += (s, e) => { };
            peerManager.OnUserquake += (s, e) => { };
            peer = new Client.Peer.Manager.Peer(peerManager);
            peer.ReadLine += Peer_ReadLine;
            peer.PeerData = new Client.Common.General.PeerData(null, 0, 0);
            peer.PeerId = () => { return 1; };
        }

        private void Peer_ReadLine(object sender, ReadLineEventArgs e)
        {
            var type = peerManager.GetType();
            var method = type.GetMethod("Peer_ReadLine", BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(peerManager, new object[] { sender, e });
        }

        [TearDown]
        public void DestroyPeer()
        {
            peerManager = null;
            peer = null;
        }

        private static IEnumerable<TestCaseData> GenerateCodeRange()
        {
            for (int i = 0; i <= 1000; i++)
            {
                yield return new TestCaseData(i);
            }
        }

        [Test, TestCaseSource("GenerateCodeRange")]
        public void CodeRangeTest(int code)
        {
            var packet = new Packet();
            packet.Code = code;
            packet.Hop = code;
            Invoke(packet);
        }

        private static IEnumerable<TestCaseData> GenerateDataRange()
        {
            return new int[] {
                551, 552, 555, 556, 561,
                611, 612, 614, 615,
                631, 632, 634, 635,
                694, 698
            }.ToList().Select(e => new TestCaseData(e));
        }

        [Test, TestCaseSource("GenerateDataRange")]
        public void RandomDataTest(int code)
        {
            var random = new Random();
            for (int i = 0; i < 100000; i++)
            {
                var packet = new Packet();
                packet.Code = code;
                packet.Hop = random.Next(100) + 1;

                var numberOfData = random.Next(10);
                if (numberOfData == 0)
                {
                    packet.Data = null;
                } else
                {
                    packet.Data = new object[numberOfData].Select(e => Guid.NewGuid().ToString("N").Substring(0,4).Replace('f',';').Replace('e',',')).ToArray();
                }

                Invoke(packet);
            }
        }

        private void Invoke(Packet packet)
        {
            var eventArgs = new ReadLineEventArgs();
            eventArgs.packet = packet;

            var type = peer.GetType();
            type.InvokeMember(
                "Socket_ReadLine",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod,
                null,
                peer,
                new object[] { this, eventArgs }
                );
        }

        [TestCase]
        public void RelayCodeTest()
        {
            var reservedCodes = new List<int>();
            reservedCodes = reservedCodes
                .Concat(Enumerable.Range(550, 1))
                .Concat(Enumerable.Range(553, 2))
                .Concat(Enumerable.Range(557, 4))
                .Concat(Enumerable.Range(562, 28))
                .Concat(Enumerable.Range(620, 10))
                .Concat(Enumerable.Range(640, 10))
                .ToList();

            var nonReservedCodes = new List<int>();
            nonReservedCodes = nonReservedCodes
                .Concat(Enumerable.Range(590, 21))
                .Concat(Enumerable.Range(613, 1))
                .Concat(Enumerable.Range(616, 4))
                .Concat(Enumerable.Range(630, 1))
                .Concat(Enumerable.Range(633, 1))
                .Concat(Enumerable.Range(636, 4))
                .Concat(Enumerable.Range(650, 44))
                .Concat(Enumerable.Range(695, 5))
                .ToList();

            var relayDatas = new List<string>
            {
                "551 1 ABC:2022/01/01 00-00-00:ABC:ABC",
                "552 1 ABC:2022/01/01 00-00-00:ABC",
                "555 1 ABC:2022/01/01 00-00-00:ABC:ABC:2022/01/01 00-00-00:ABC",
                "556 1 ABC:2022/01/01 00-00-00:ABC:ABC:2022/01/01 00-00-00:ABC",
                "561 1 ABC:2022/01/01 00-00-00:ABC",
                "615 1 1:1",
                "635 1 1:1:2:3:4",
            }
            .Concat(reservedCodes.Select(code => $"{code} 1"))
            .ToList();

            var denyDatas = new List<string>
            {
                "611 1",
                "612 1",
                "614 1 0.35:ABC:ABC",
                "631 1",
                "632 1 1",
                "634 1 0.35:ABC:ABC",
                "694 1",
            }
            .Concat(nonReservedCodes.Select(code => $"{code} 1"))
            .ToList();

            var invokeCount = 0;
            peer.ReadLine += (s, e) => { invokeCount++; };

            Assert.Multiple(() =>
            {
                foreach (var relayData in relayDatas)
                {
                    invokeCount = 0;
                    Invoke(Packet.Parse(relayData));
                    Assert.AreEqual(1, invokeCount, $"Not relayed: {relayData}");
                }

                foreach (var denyData in denyDatas)
                {
                    invokeCount = 0;
                    Invoke(Packet.Parse(denyData));
                    Assert.AreEqual(0, invokeCount, $"Relayed: {denyData}");
                }
            });
        }
    }
}
