using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Client.App;
using Client.Client.General;
using Client.Common.Net;
using Client.Peer;
using Moq;
using NUnit.Framework;
using PKCSPeerCrypto;

namespace ClientTest.App
{
    [TestFixture]
    class MediatorContextUserquakeTest
    {
        string privateKey = "MIIBBwIBADANBgkqhkiG9w0BAQEFAASB8jCB7wIBAAIxALDbI8NYvXosNN9SY0CzCq8c8Zv2UJpSt3M90Le+nKaaTyypzOKWFOQvNaoDNFbW2QIBEQIwE4GU4rjYqbXRICkWPblk8W58h8o51MkfWAlam11rgkp9xRGJpsBGP0Nxwh3zhvzBAhkA4mKz41EEqRnU7zv7efKRx09hsBlu9nppAhkAx/3FMacek2WfhvDxXqEGNJ53PA1pqYrxAhhqiM0frafXGzcHK0kqVAhdywDLV0NG0DECGF4dEYDHHXKKLPQ1JkqmIQmz++AGT9dQcQIYEEH0zolfobS0qHZk7GAyZh0kb6QSAx7/";
        string publicKey = "MEowDQYJKoZIhvcNAQEBBQADOQAwNgIxALDbI8NYvXosNN9SY0CzCq8c8Zv2UJpSt3M90Le+nKaaTyypzOKWFOQvNaoDNFbW2QIBEQ==";
        string keyExpire = "2017/04/04 23-37-41";
        string keySignature = "ttZhe/dyH7RioGsJx9IXHfwEdkeHe7UJIHqflpaPFAOQREvTZToU9/sq0D2LB8DDnywCyf+kaC4EfMduWl//joel8f+w0D7RiV2EX3eStIIyHTncbvi/HhdAu9PcZS2/5BZ1aIFY7YXbr8VjVq88TL72tDZxzkNlV1fgfsmFIBk=";

        MediatorContext mediatorContext;
        Mock<IPeerContext> peerContextMock;

        [SetUp]
        public void SetUp()
        {
            mediatorContext = new MediatorContext();
            mediatorContext.PeerId = 10;
            mediatorContext.AreaCode = 901;

            var key = new KeyData();
            key.Expire = DateTime.Parse(keyExpire.Replace("-", ":"));
            key.PublicKey = publicKey;
            key.PrivateKey = privateKey;
            key.Signature = keySignature;
            mediatorContext.Key = key;
            mediatorContext.TimeOffset = key.Expire.Subtract(DateTime.Now.AddMinutes(1));

            peerContextMock = new Mock<IPeerContext>();
            var field = mediatorContext.GetType().GetField("peerContext", BindingFlags.SetField | BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(mediatorContext, peerContextMock.Object);
        }

        [TestCase]
        public void SendUserquake_Valid()
        {
            Packet packet = null;
            peerContextMock.Setup(x => x.SendAll(It.IsAny<Packet>())).Callback<Packet>((e) =>
            {
                packet = e;
            });
            
            Assert.IsTrue(mediatorContext.SendUserquake());
            
            Assert.IsNotNull(packet);
            
            Verifier.VerifyResult result = Verifier.VerifyUserquake(packet.Data[5], packet.Data[1], packet.Data[0], packet.Data[2], packet.Data[3], packet.Data[4], mediatorContext.CalcNowProtocolTime());
            Assert.IsFalse(result.isExpired);
            Assert.IsTrue(result.isValidSignature);
            Assert.AreEqual("901", packet.Data[5].Split(',')[1]);
        }

        [TestCase]
        public void SendUserquake_Expired()
        {
            mediatorContext.TimeOffset = mediatorContext.Key.Expire.Subtract(DateTime.Now.AddSeconds(-1));

            Packet packet = null;
            peerContextMock.Setup(x => x.SendAll(It.IsAny<Packet>())).Callback<Packet>((e) =>
            {
                packet = e;
            });

            Assert.IsFalse(mediatorContext.SendUserquake());

            Assert.IsNull(packet);
        }

        [TestCase]
        public void SendUserquake_NoKey()
        {
            mediatorContext.Key = null;

            Packet packet = null;
            peerContextMock.Setup(x => x.SendAll(It.IsAny<Packet>())).Callback<Packet>((e) =>
            {
                packet = e;
            });

            Assert.IsFalse(mediatorContext.SendUserquake());

            Assert.IsNull(packet);
        }

        [TestCase]
        public void SendUserquake_NotConfigured()
        {
            mediatorContext.PeerId = 0;
            mediatorContext.AreaCode = 0;

            Packet packet = null;
            peerContextMock.Setup(x => x.SendAll(It.IsAny<Packet>())).Callback<Packet>((e) =>
            {
                packet = e;
            });

            Assert.IsFalse(mediatorContext.SendUserquake());

            Assert.IsNull(packet);
        }
    }
}
