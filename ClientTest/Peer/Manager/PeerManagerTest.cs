using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Client.Common.Net;
using Client.Peer.Manager;
using Moq;
using NUnit.Framework;

namespace ClientTest.Peer.Manager
{
    [TestFixture]
    class PeerManagerTest
    {
        [TestCase]
        public void DisconnectPeerWhileSendingPacket()
        {
            var peerManager = new PeerManager();
            peerManager.ConnectionsChanged += (s, e) => { };

            var peerList = new List<Client.Peer.Manager.Peer>();

            int callCount = 0;
            for (int i = 0; i < 5; i++)
            {
                var crlfSocket = new CRLFSocket();

                var peer = new Client.Peer.Manager.Peer(peerManager, crlfSocket);
                peerList.Add(peer);

                var mockSocket = new Mock<ISocket>();
                mockSocket.SetupGet(x => x.Connected).Returns(true);
                mockSocket.Setup(x => x.Send(It.IsAny<byte[]>())).Callback(() =>
                {
                    callCount++;
                    Console.WriteLine(callCount);

                    if (callCount == 2)
                    {
                        var socketClose = peerManager.GetType().GetMethod("peer_Closed", BindingFlags.NonPublic | BindingFlags.Instance);
                        socketClose.Invoke(peerManager, new object[] { peer, EventArgs.Empty });
                    }
                });

                var crlfField = crlfSocket.GetType().GetField("socket", BindingFlags.SetField | BindingFlags.NonPublic | BindingFlags.Instance);
                crlfField.SetValue(crlfSocket, mockSocket.Object);
            }

            var field = peerManager.GetType().GetField("peerList", BindingFlags.SetField | BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(peerManager, peerList);
            
            var packet = new Packet();
            packet.Code = 211;
            packet.Hop = 1;

            peerManager.Send(packet);
        }
    }
}
