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
    }
}
