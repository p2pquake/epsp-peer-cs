using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Client.Common.Net;
using Moq;
using NUnit.Framework;

namespace ClientTest.Common.Net
{
    [TestFixture]
    class CRLFSocketTest
    {
        [TestCase]
        public void MoqMockingTest()
        {
            var crlfSocket = new CRLFSocket();
            var field = crlfSocket.GetType().GetField("socket", BindingFlags.SetField | BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.AreEqual(ConnectionState.Closed, crlfSocket.State);

            var mockSocket = new Mock<ISocket>();
            mockSocket.SetupGet(x => x.Connected).Returns(true);
            field.SetValue(crlfSocket, mockSocket.Object);

            Assert.AreEqual(ConnectionState.Connected, crlfSocket.State);
        }

        [TestCase]
        public void DisposedExceptionTest()
        {
            var crlfSocket = new CRLFSocket();
            var field = crlfSocket.GetType().GetField("socket", BindingFlags.SetField | BindingFlags.NonPublic | BindingFlags.Instance);

            var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            var mockSocket = new Mock<SocketAdapter>(socket) { CallBase = true };
            mockSocket.SetupGet(x => x.Connected).Returns(true);
            field.SetValue(crlfSocket, mockSocket.Object);

            socket.Dispose();
            Assert.IsFalse(crlfSocket.WriteLine("611 1"));
        }
    }
}
