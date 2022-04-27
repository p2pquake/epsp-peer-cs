using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
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
        private Random random = new Random();

        [TestCase]
        public void PeerReadLineDuplicateRemoving()
        {
            var peerManager = new PeerManager()
            {
                PeerCount = () => 100,
                PeerId = () => 1,
                ProtocolTime = () => DateTime.Now,
            };
            peerManager.ConnectionsChanged += (s, e) => { };

            var count = 0;
            peerManager.OnAreapeers += (s, e) => { count++; };

            var line = "561 1 ABC:2022/01/01 00-00-00:001,100";
            var packet = Packet.Parse(line);
            var eventArgs = new ReadLineEventArgs() { line = line, packet = packet };

            var readLine = peerManager.GetType().GetMethod("Peer_ReadLine", BindingFlags.Instance | BindingFlags.NonPublic);

            Enumerable.Range(0, 5).ToList().ForEach(x => readLine.Invoke(peerManager, new object[] { null, eventArgs }));

            Assert.AreEqual(1, count);
        }

        [TestCase]
        public void PeerReadLineHopLimit()
        {
            var peerManager = new PeerManager()
            {
                PeerCount = () => 9,
                PeerId = () => 1,
                ProtocolTime = () => DateTime.Now,
            };
            peerManager.ConnectionsChanged += (s, e) => { };

            var readLine = peerManager.GetType().GetMethod("Peer_ReadLine", BindingFlags.Instance | BindingFlags.NonPublic);
            void fn(string line) => readLine.Invoke(peerManager, new object[] { null, new ReadLineEventArgs() { line = line, packet = Packet.Parse(line) } });    

            var count = 0;
            peerManager.OnAreapeers += (s, e) => { count++; };

            fn("561 9 ABC:2022/01/01 00-00-09:001,100");
            fn("561 10 ABC:2022/01/01 00-00-10:001,100");
            fn("561 11 ABC:2022/01/01 00-00-11:001,100");
            Assert.AreEqual(2, count);

            peerManager.PeerCount = () => 400;
            fn("561 20 ABC:2022/01/01 00-00-08:001,100");
            fn("561 21 ABC:2022/01/01 00-00-09:001,100");
            Assert.AreEqual(3, count);
        }

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
                        var socketClose = peerManager.GetType().GetMethod("Peer_Closed", BindingFlags.NonPublic | BindingFlags.Instance);
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

        [TestCase]
        public void NetworkInquiryTest()
        {
            var peerManager = new PeerManager();
            peerManager.ConnectionsChanged += (s, e) => { };
            peerManager.PeerCount += () => { return 1000; };
            peerManager.PeerId += () => { return 55; };

            var peerList = new List<Client.Peer.Manager.Peer>();

            int callCount = 0;
            for (int i = 0; i < 5; i++)
            {
                var crlfSocket = new CRLFSocket();

                var peer = new Client.Peer.Manager.Peer(peerManager, crlfSocket);
                peer.PeerData = new Client.Common.General.PeerData("127.0.0." + (i + 1).ToString(), 6900 + i, 100 + i);
                peer.PeerId += () => { return peerManager.PeerId(); };
                peerList.Add(peer);

                var mockSocket = new Mock<ISocket>();
                mockSocket.SetupGet(x => x.Connected).Returns(true);
                mockSocket.Setup(x => x.Send(It.IsAny<byte[]>())).Callback<byte[]>((k) =>
                {
                    var line = Encoding.ASCII.GetString(k);
                    if (line.StartsWith("611"))
                    {
                        return;
                    }

                    callCount++;
                    if (callCount < 5)
                    {
                        Assert.AreEqual("615 6 200:300\r\n", line);
                    }
                    if (callCount == 5)
                    {
                        Assert.AreEqual("635 1 200:300:55:100,101,102,103,104:5\r\n", line);
                    }
                });

                var crlfField = crlfSocket.GetType().GetField("socket", BindingFlags.SetField | BindingFlags.NonPublic | BindingFlags.Instance);
                crlfField.SetValue(crlfSocket, mockSocket.Object);
            }

            var field = peerManager.GetType().GetField("peerList", BindingFlags.SetField | BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(peerManager, peerList);

            var packet = new Packet();
            packet.Code = 615;
            packet.Hop = 5;
            packet.Data = new string[] { "200", "300" };

            var eventArgs = new ReadLineEventArgs();
            eventArgs.line = packet.ToPacketString();
            eventArgs.packet = packet;

            var type = peerManager.GetType();
            type.InvokeMember(
                "Peer_ReadLine",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.InvokeMethod,
                null,
                peerManager,
                new object[] { peerList[0], eventArgs }
                );

            Assert.AreEqual(5, callCount);
        }

        [TestCase]
        public void PeerListMultithreadOperationTest()
        {
            var peerManager = new PeerManager()
            {
                PeerCount = () => 1000,
                PeerId = () => 55,
            };
            peerManager.ConnectionsChanged += (s, e) => { };

            /*
             * Connect randomly
             */
            var tcpListener = new TcpListener(IPAddress.Any, 9999);
            tcpListener.Start();
            var sockets = new List<Socket>();
            Task.Run(async () =>
            {
                while (true)
                {
                    var socket = await tcpListener.AcceptSocketAsync();
                    lock (sockets) { sockets.Add(socket); }
                }
            });

            Action connect = () =>
            {
                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    socket.Connect(IPEndPoint.Parse($"127.0.{random.Next(255)}.1:9999"));
                } catch (SocketException)
                {
                    // nothing to do
                }
                peerManager.AddFromSocket(socket);
            };

            /*
             * Disconnect randomly
             */
            Action disconnect = () =>
            {
                if (random.Next() > 0.5) { return; }
                lock (sockets)
                {
                    if (sockets.Count() > 0)
                    {
                        sockets[random.Next(sockets.Count())].Close(); ;
                    }
                }
            };

            /*
             * DisconnectAll randomly
             */
            Action disconnectAll = () =>
            {
                if (random.Next() > 0.2) { return; }
                peerManager.DisconnectAll();
            };

            /*
             * Send randomly
             */
            Action sendAll = () =>
            {
                peerManager.Send(new Packet() { Code = 611, Hop = 1, Data = new string[] { } });
            };

            /*
             * Receive randomly
             */

            /*
             * State change randomly
             */

            var actions = new List<Action>()
            {
                connect,
                disconnect,
                disconnectAll,
                sendAll,
            };
            actions = Enumerable.Repeat(actions, 100).SelectMany(e => e).ToList();

            var sw = new Stopwatch();
            sw.Start();
            Parallel.Invoke(actions.Select(e => new Action(() =>
            {
                while (sw.ElapsedMilliseconds < 10000)
                {
                    e();
                }
            })).ToArray());
        }
    }
}
