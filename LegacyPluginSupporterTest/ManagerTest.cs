using LegacyPluginSupporter;

using System.Net.Sockets;
using System.Text;

namespace LegacyPluginSupporterTest
{
    public class ManagerTest
    {
        private Manager manager;

        [SetUp]
        public void Setup()
        {
            manager = new Manager("プラグインテスト");
            if (!manager.Listen())
            {
                Assert.Fail("ポート Listen に失敗");
            }
        }

        [TearDown]
        public void TearDown()
        {
            manager.Shutdown();
        }

        [Test]
        public void Connect()
        {
            var tcpClient = new TcpClient();
            tcpClient.Connect("localhost", 6918);
        }

        [Test]
        public void InitialCommunication()
        {
            var tcpClient = new TcpClient();
            tcpClient.Connect("localhost", 6918);

            var notified = false;
            manager.OnNameNotified += (s, e) =>
            {
                Assert.That(manager.PluginList[0].PluginName, Is.EqualTo("テストプラグイン"));
                notified = true;
            };

            var stream = tcpClient.GetStream();
            stream.Write(Encoding.GetEncoding(932).GetBytes("JOIN テストプラグイン\r\n"));

            var message = readFromStream(stream);
            Assert.That(message, Is.EqualTo("SVER プラグインテスト\r\n"));
            Assert.That(() => notified, Is.True.After(1000, 100));
        }

        [Test]
        public void RaiseEvents()
        {
            var userquakeRequested = false;
            var exitRequested = false;
            manager.OnUserquakeRequest += (s, e) => { userquakeRequested = true; };
            manager.OnExitRequest += (s, e) => { exitRequested = true; };

            var tcpClient = new TcpClient();
            tcpClient.Connect("localhost", 6918);

            var stream = tcpClient.GetStream();
            stream.Write(Encoding.GetEncoding(932).GetBytes("USRQ\r\n"));
            Assert.That(() => userquakeRequested, Is.True.After(1000, 100));

            stream.Write(Encoding.GetEncoding(932).GetBytes("EXIT\r\n"));
            Assert.That(() => exitRequested, Is.True.After(1000, 100));
        }

        [Test]
        public void SendEvents()
        {
            var tcpClient = new TcpClient();
            tcpClient.Connect("localhost", 6918);

            var stream = tcpClient.GetStream();
            stream.Write(Encoding.GetEncoding(932).GetBytes("JOIN テストプラグイン\r\n"));

            var message = readFromStream(stream);
            Assert.That(message, Is.EqualTo("SVER プラグインテスト\r\n"));

            manager.RequestOption(manager.PluginList[0]);
            var optiMessage = readFromStream(stream);
            Assert.That(optiMessage, Is.EqualTo("OPTI\r\n"));

            manager.NotifyEarthquake(new Client.Peer.EPSPQuakeEventArgs()
            {
                RawAbstractString = "概要",
                RawDetailString = "詳細",
            });
            var quakMessage = readFromStream(stream);
            Assert.That(quakMessage, Is.EqualTo("QUAK 概要:詳細\r\n"));

            manager.NotifyTsunami(new Client.Peer.EPSPTsunamiEventArgs()
            {
                RawDetailString = "解除",
            });
            var tidlMessage = readFromStream(stream);
            Assert.That(tidlMessage, Is.EqualTo("TIDL 解除\r\n"));

            manager.NotifyEEW(new Client.Peer.EPSPEEWEventArgs()
            {
                IsTest = false,
            });
            var eew1Message = readFromStream(stream);
            Assert.That(eew1Message, Is.EqualTo("EEW1 0\r\n"));
        }

        private string readFromStream(NetworkStream stream)
        {
            var buffer = new byte[1023];
            var readBytes = stream.Read(buffer);
            return Encoding.GetEncoding(932).GetString(buffer, 0, readBytes);
        }
    }
}