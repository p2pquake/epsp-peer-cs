using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Client.Common.Net;
using NUnit.Framework;

namespace ClientTest.Common.Net
{
    [TestFixture]
    class CRLFSocketParseTest
    {
        CRLFSocket crlfSocket;

        [SetUp]
        public void SetUp()
        {
            crlfSocket = new CRLFSocket();
        }

        [TearDown]
        public void TearDown()
        {
            crlfSocket = null;
        }
        
        [TestCase]
        public void ParseOneLine()
        {
            var called = false;
            crlfSocket.ReadLine += (s, e) =>
            {
                called = true;
                Assert.AreEqual("611 1", e.line);
                Assert.AreEqual(611, e.packet.Code);
                Assert.AreEqual(1, e.packet.Hop);
                Assert.IsNull(e.packet.Data);
            };

            Invoke("611 1\r\n");
            Assert.IsTrue(called);
        }

        [TestCase]
        public void ParseOneLineByMultiplePackets()
        {
            crlfSocket.ReadLine += CRLFSocket_FailRead;
            Invoke("611 1");
            
            var called = false;
            crlfSocket.ReadLine -= CRLFSocket_FailRead;
            crlfSocket.ReadLine += (s, e) =>
            {
                called = true;
                Assert.AreEqual("611 1", e.line);
                Assert.AreEqual(611, e.packet.Code);
                Assert.AreEqual(1, e.packet.Hop);
                Assert.IsNull(e.packet.Data);
            };

            Invoke("\r\n");
            Assert.IsTrue(called);
        }

        [TestCase]
        public void ParseTwoLines()
        {
            int callCount = 0;
            crlfSocket.ReadLine += (s, e) =>
            {
                callCount++;

                if (callCount == 1)
                {
                    Assert.AreEqual("611 1", e.line);
                    Assert.AreEqual(611, e.packet.Code);
                    Assert.AreEqual(1, e.packet.Hop);
                    Assert.IsNull(e.packet.Data);
                }
                if (callCount == 2)
                {
                    Assert.AreEqual("631 2", e.line);
                    Assert.AreEqual(631, e.packet.Code);
                    Assert.AreEqual(2, e.packet.Hop);
                    Assert.IsNull(e.packet.Data);
                }
            };

            Invoke("611 1\r\n631 2\r\n");
            Assert.AreEqual(2, callCount);
        }

        [TestCase]
        public void ParseThreeLines()
        {
            int callCount = 0;
            crlfSocket.ReadLine += (s, e) =>
            {
                callCount++;

                if (callCount == 1)
                {
                    Assert.AreEqual("611 1", e.line);
                    Assert.AreEqual(611, e.packet.Code);
                    Assert.AreEqual(1, e.packet.Hop);
                    Assert.IsNull(e.packet.Data);
                }
                if (callCount == 2)
                {
                    Assert.AreEqual("631 2", e.line);
                    Assert.AreEqual(631, e.packet.Code);
                    Assert.AreEqual(2, e.packet.Hop);
                    Assert.IsNull(e.packet.Data);
                }
                if (callCount == 3)
                {
                    Assert.AreEqual("611 5", e.line);
                    Assert.AreEqual(611, e.packet.Code);
                    Assert.AreEqual(5, e.packet.Hop);
                    Assert.IsNull(e.packet.Data);
                }
            };

            Invoke("611 1\r\n631 2\r\n611 5\r\n");
            Assert.AreEqual(3, callCount);
        }

        [TestCase]
        public void ParseTwoLinesByMultiplePackets()
        {
            crlfSocket.ReadLine += CRLFSocket_FailRead;
            Invoke("635 4 3343:1000419708:594:2399,2844,2368:7");

            int callCount = 0;
            crlfSocket.ReadLine -= CRLFSocket_FailRead;
            crlfSocket.ReadLine += (s, e) =>
            {
                callCount++;

                if (callCount == 1)
                {
                    Assert.AreEqual(635, e.packet.Code);
                    Assert.AreEqual(4, e.packet.Hop);
                }
                if (callCount == 2)
                {
                    Assert.AreEqual(635, e.packet.Code);
                    Assert.AreEqual(5, e.packet.Hop);
                }
            };

            Invoke("\r\n635 5 3343:1000419708:803:2412,3715,3287:8\r\n");
            Assert.AreEqual(2, callCount);
        }

        [TestCase]
        public void ParseTwoLinesByMultiplePacketsCR()
        {
            crlfSocket.ReadLine += CRLFSocket_FailRead;
            Invoke("635 4 3343:1000419708:594:2399,2844,2368:7\r");

            int callCount = 0;
            crlfSocket.ReadLine -= CRLFSocket_FailRead;
            crlfSocket.ReadLine += (s, e) =>
            {
                callCount++;

                if (callCount == 1)
                {
                    Assert.AreEqual(635, e.packet.Code);
                    Assert.AreEqual(4, e.packet.Hop);
                }
                if (callCount == 2)
                {
                    Assert.AreEqual(635, e.packet.Code);
                    Assert.AreEqual(5, e.packet.Hop);
                }
            };

            Invoke("\n635 5 3343:1000419708:803:2412,3715,3287:8\r\n");
            Assert.AreEqual(2, callCount);
        }

        private void CRLFSocket_FailRead(object sender, ReadLineEventArgs e)
        {
            Assert.Fail("ReadLineイベントを発生させてはいけない");
        }

        private void Invoke(string data)
        {
            var field = crlfSocket.GetType().GetField("receiveBuffer", BindingFlags.SetField | BindingFlags.NonPublic | BindingFlags.Instance);
            var method = crlfSocket.GetType().GetMethod("ProcessReceiveData", BindingFlags.NonPublic | BindingFlags.Instance);

            field.SetValue(crlfSocket, Encoding.GetEncoding(932).GetBytes(data));
            method.Invoke(crlfSocket, new object[] { Encoding.GetEncoding(932).GetBytes(data).Length });
        }
    }
}
