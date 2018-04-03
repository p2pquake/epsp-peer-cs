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
        Client.Peer.Manager.Peer peer;

        [SetUp]
        public void CreatePeer()
        {
            peer = new Client.Peer.Manager.Peer(new Client.Peer.Manager.PeerManager());
        }

        [TearDown]
        public void DestroyPeer()
        {
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

        private void Invoke(Packet packet)
        {
            var eventArgs = new ReadLineEventArgs();
            eventArgs.packet = packet;

            var type = peer.GetType();
            type.InvokeMember(
                "socket_ReadLine",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod,
                null,
                peer,
                new object[] { this, eventArgs }
                );
        }
    }
}
