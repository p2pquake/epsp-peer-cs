using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.Peer.Manager;
using NUnit.Framework;

namespace ClientTest.Peer.Manager
{
    [TestFixture]
    class NetworkInquiryManagerTest
    {
        NetworkInquiryManager networkInquiryManager;

        [SetUp]
        public void SetUp()
        {
            networkInquiryManager = new NetworkInquiryManager();
        }

        [Test]
        public void NullTest()
        {
            Assert.IsNull(networkInquiryManager.FindPeer(null, null));
        }

        [Test]
        public void EmptyTest()
        {
            Assert.IsNull(networkInquiryManager.FindPeer(string.Empty, string.Empty));
        }

        [Test]
        public void SimpleFindTest()
        {
            var peer = new Client.Peer.Manager.Peer(null);
            networkInquiryManager.Add("100", "200", peer);
            Assert.AreSame(peer, networkInquiryManager.FindPeer("100", "200"));
        }

        [Test]
        public void CapacityBoundaryTest()
        {
            var peer = new Client.Peer.Manager.Peer(null);
            networkInquiryManager.Add("100", "200", peer);

            foreach (int i in Enumerable.Range(0, 99))
            {
                networkInquiryManager.Add("200", "300", new Client.Peer.Manager.Peer(null));
            }
            Assert.AreSame(peer, networkInquiryManager.FindPeer("100", "200"));

            networkInquiryManager.Add("200", "300", new Client.Peer.Manager.Peer(null));
            Assert.IsNull(networkInquiryManager.FindPeer("100", "200"));
        }
    }
}
