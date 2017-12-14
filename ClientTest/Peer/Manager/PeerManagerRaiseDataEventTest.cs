using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.Common.Net;
using Client.Peer;
using Client.Peer.Manager;
using NUnit.Framework;

namespace ClientTest.Peer.Manager
{
    [TestFixture]
    class PeerManagerRaiseDataEventTest
    {
        private PeerManager peerManager;

        [SetUp]
        public void createPeerManager()
        {
            peerManager = new PeerManager();
        }

        [TearDown]
        public void destroyPeerManager()
        {
            peerManager = null;
        }

        private void invokeRaiseDataEvent(string packetString)
        {
            var type = peerManager.GetType();
            type.InvokeMember(
                "raiseDataEvent",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.InvokeMethod,
                null,
                peerManager,
                new object[] { Packet.Parse(packetString) }
                );
        }

        [TestCase]
        public void raiseDataEvent_AreapeersData_VerificationSucceeded()
        {
            Assert.Ignore("Not Implemented");
        }

        [TestCase]
        public void raiseDataEvent_AreapeersData_RaiseOnAreapeers()
        {
            var retreive =
                "561 10 fLLhdQwhiy+wT9SaNwRIBIT9cAnD6DsupOpiVUXeK0CIoBg8Fo83q/xrIDGxHSn7lRvn2F8XINdgALm1akthxHWXDwInFS9+BEut2DDnY5tOoh3HxZS1XTCJC+qif5ebY6UUCV/AIbJFTKiZYS9hlrymj23ZquXMiDR563Au+1g=:2017/12/13 22-08-08:010,61;015,8;025,2;030,6;035,8;040,1;045,3;050,6;055,10;060,2;065,13;070,7;075,2;100,24;105,18;106,1;110,8;111,6;115,37;120,32;125,62;130,23;135,16;140,5;141,2;142,18;143,6;150,35;151,11;152,8;200,38;205,76;210,7;215,45;220,8;225,49;230,38;231,197;232,5;240,22;241,154;242,10;250,667;255,1;270,275;275,67;300,3;301,18;302,22;310,8;315,8;325,12;330,5;335,1;340,5;345,13;350,18;351,15;355,10;405,32;410,6;411,42;415,54;416,30;420,26;425,115;430,28;435,3;440,4;445,11;450,2;455,27;460,88;465,34;470,4;475,68;480,19;490,13;495,4;500,2;505,5;510,9;515,2;520,2;525,29;530,2;535,21;541,7;545,5;550,8;555,1;560,27;570,6;575,8;576,3;580,2;581,11;600,35;601,12;602,1;605,4;610,1;615,2;620,1;625,5;641,14;646,2;650,3;651,8;656,1;660,3;665,7;670,13;675,4;685,1;700,1;701,3;900,101;901,5;905,4";
            var expected =
                new Dictionary<string, int>()
                {
                    { "010", 61 },
                    { "015", 8 },
                    { "025", 2 },
                    { "030", 6 },
                    { "035", 8 },
                    { "040", 1 },
                    { "045", 3 },
                    { "050", 6 },
                    { "055", 10 },
                    { "060", 2 },
                    { "065", 13 },
                    { "070", 7 },
                    { "075", 2 },
                    { "100", 24 },
                    { "105", 18 },
                    { "106", 1 },
                    { "110", 8 },
                    { "111", 6 },
                    { "115", 37 },
                    { "120", 32 },
                    { "125", 62 },
                    { "130", 23 },
                    { "135", 16 },
                    { "140", 5 },
                    { "141", 2 },
                    { "142", 18 },
                    { "143", 6 },
                    { "150", 35 },
                    { "151", 11 },
                    { "152", 8 },
                    { "200", 38 },
                    { "205", 76 },
                    { "210", 7 },
                    { "215", 45 },
                    { "220", 8 },
                    { "225", 49 },
                    { "230", 38 },
                    { "231", 197 },
                    { "232", 5 },
                    { "240", 22 },
                    { "241", 154 },
                    { "242", 10 },
                    { "250", 667 },
                    { "255", 1 },
                    { "270", 275 },
                    { "275", 67 },
                    { "300", 3 },
                    { "301", 18 },
                    { "302", 22 },
                    { "310", 8 },
                    { "315", 8 },
                    { "325", 12 },
                    { "330", 5 },
                    { "335", 1 },
                    { "340", 5 },
                    { "345", 13 },
                    { "350", 18 },
                    { "351", 15 },
                    { "355", 10 },
                    { "405", 32 },
                    { "410", 6 },
                    { "411", 42 },
                    { "415", 54 },
                    { "416", 30 },
                    { "420", 26 },
                    { "425", 115 },
                    { "430", 28 },
                    { "435", 3 },
                    { "440", 4 },
                    { "445", 11 },
                    { "450", 2 },
                    { "455", 27 },
                    { "460", 88 },
                    { "465", 34 },
                    { "470", 4 },
                    { "475", 68 },
                    { "480", 19 },
                    { "490", 13 },
                    { "495", 4 },
                    { "500", 2 },
                    { "505", 5 },
                    { "510", 9 },
                    { "515", 2 },
                    { "520", 2 },
                    { "525", 29 },
                    { "530", 2 },
                    { "535", 21 },
                    { "541", 7 },
                    { "545", 5 },
                    { "550", 8 },
                    { "555", 1 },
                    { "560", 27 },
                    { "570", 6 },
                    { "575", 8 },
                    { "576", 3 },
                    { "580", 2 },
                    { "581", 11 },
                    { "600", 35 },
                    { "601", 12 },
                    { "602", 1 },
                    { "605", 4 },
                    { "610", 1 },
                    { "615", 2 },
                    { "620", 1 },
                    { "625", 5 },
                    { "641", 14 },
                    { "646", 2 },
                    { "650", 3 },
                    { "651", 8 },
                    { "656", 1 },
                    { "660", 3 },
                    { "665", 7 },
                    { "670", 13 },
                    { "675", 4 },
                    { "685", 1 },
                    { "700", 1 },
                    { "701", 3 },
                    { "900", 101 },
                    { "901", 5 },
                    { "905", 4 }
                };
            
            bool called = false;
            peerManager.OnAreapeers += (s, e) =>
            {
                Assert.AreEqual(expected, e.AreaPeerDictionary);
                called = true;
            };
            invokeRaiseDataEvent(retreive);

            Assert.IsTrue(called);
        }

        [TestCase]
        public void raiseDataEvent_EarthquakeData_VerificationSucceeded()
        {
            Assert.Ignore("Not Implemented");
        }

        [TestCase]
        public void raiseDataEvent_EarthquakeData_RaiseOnEarthquake()
        {
            Assert.Ignore("Not Implemented");
        }

        [TestCase]
        public void raiseDataEvent_TsunamiData_VerificationSucceeded()
        {
            Assert.Ignore("Not Implemented");
        }

        [TestCase]
        public void raiseDataEvent_TsunamiData_RaiseOnTsunami_MajorWarning()
        {
            Assert.Ignore("Not Implemented");
        }

        [TestCase]
        public void raiseDataEvent_TsunamiData_RaiseOnTsunami_Warning()
        {
            var retreive =
                "552 9 e6hJ/b2CCzDlY7UUbLH+umg/NR5QFeZ3nTdlmXRwHVVFrtJK2NkrqCva3gL7sGmKyxksYeR423PYjSpjGJvdE2TrIu2tmB0+vQsxgUOiqiZqv89znioza2W6eZ9oy3+TlsU5s8ENYR92U3IgzIJp5YpvlaIsx/wGqqWdnbpV6Ns=:2016/11/22 08-14-33:-津波警報,+宮城県,+福島県,-津波注意報,+青森県太平洋沿岸,+岩手県,+茨城県,+千葉県九十九里・外房,+千葉県内房,+伊豆諸島";

            bool called = false;
            peerManager.OnTsunami += (s, e) =>
            {
                called = true;
                Assert.IsFalse(e.IsCancelled);

                object[,] expected =
                {
                    { TsunamiCategory.Warning, false, "宮城県" },
                    { TsunamiCategory.Warning, false, "福島県" },
                    { TsunamiCategory.Advisory, false, "青森県太平洋沿岸" },
                    { TsunamiCategory.Advisory, false, "岩手県" },
                    { TsunamiCategory.Advisory, false, "茨城県" },
                    { TsunamiCategory.Advisory, false, "千葉県九十九里・外房" },
                    { TsunamiCategory.Advisory, false, "千葉県内房" },
                    { TsunamiCategory.Advisory, false, "伊豆諸島" }
                };
                Assert.AreEqual(expected.GetLength(0), e.RegionList.Count);

                for (int i = 0; i < expected.GetLength(0); i++)
                {
                    Assert.AreEqual(expected[i, 0], e.RegionList[i].Category);
                    Assert.AreEqual(expected[i, 1], e.RegionList[i].IsImmediately);
                    Assert.AreEqual(expected[i, 2], e.RegionList[i].Region);
                }
            };
            invokeRaiseDataEvent(retreive);

            Assert.IsTrue(called);
        }

        [TestCase]
        public void raiseDataEvent_TsunamiData_RaiseOnTsunami_Advisory()
        {
            var retreive =
                "552 11 Oqqn36REt0RjpKqBibXn6A1woRon/xQ7ySYVmET24n3gxaKAEBE/e+eyQ9TUAAePDEkyUc/pGB+S3w8vyHE6gYRuCNx9ysC7gsdSNh/Bv4C3T3p0WDv2GtJq1jSETkcZNkKv9p5Ptny5+kHt9y5dPA/enZQ7yPCul5EezHsxM80=:2016/11/22 07-30-21:-津波注意報,*千葉県内房,*伊豆諸島";

            bool called = false;
            peerManager.OnTsunami += (s, e) =>
            {
                called = true;
                Assert.IsFalse(e.IsCancelled);

                object[,] expected =
                {
                    { TsunamiCategory.Advisory, true, "千葉県内房" },
                    { TsunamiCategory.Advisory, true, "伊豆諸島" }
                };
                Assert.AreEqual(expected.GetLength(0), e.RegionList.Count);
                
                for (int i = 0; i < expected.GetLength(0); i++)
                {
                    Assert.AreEqual(expected[i,0], e.RegionList[i].Category);
                    Assert.AreEqual(expected[i,1], e.RegionList[i].IsImmediately);
                    Assert.AreEqual(expected[i,2], e.RegionList[i].Region);
                }
            };
            invokeRaiseDataEvent(retreive);

            Assert.IsTrue(called);
        }

        [TestCase]
        public void raiseDataEvent_TsunamiData_RaiseOnTsunami_Cancelled()
        {
            var retreive =
                "552 8 CU2Au7WAvqoiKXaAfmEEXS2W4vvePShT5W6cXXSsTpSfd9OA23Oc0GulQRr5D+mJ2t9mnOATVml5F7jwlGOMugMSFR49cmhFSIlLg3eDisotDvKIsNXpr//T3vcF9UiZA5JJzzcaXZuWg1591CXBN/xkWJf4C73LE3HmQewtR0w=:2016/11/22 12-53-43:解除";

            bool called = false;
            peerManager.OnTsunami += (s, e) =>
            {
                called = true;
                Assert.IsTrue(e.IsCancelled);
                Assert.IsTrue(e.RegionList == null || e.RegionList.Count == 0);
            };
            invokeRaiseDataEvent(retreive);

            Assert.IsTrue(called);
        }

        [TestCase]
        public void raiseDataEvent_UserquakeData_VerificationSucceeded()
        {
            Assert.Ignore("Not Implemented");
        }

        [TestCase]
        public void raiseDataEvent_UserquakeData_RaiseOnUserquake()
        {
            var retreive =
                "555 7 IyZUbE0C5de3CrUYIN+LqhsG6C3+E1dNoxD1Ef02qiri4qW7lqPb5J2p1Fhe3rZs:2017/12/14 06-39-30:MEwwDQYJKoZIhvcNAQEBBQADOwAwOAIxAKK/e5GQjXQPWQRhsMxTmOfQ9ISxDKb747F5g80s9wlB4XIjH7Ig8bycZp90vz3WzQIDAQAB:j74KAyS6Yy5LrEzm33OGBMMkhXT14n6bKajP4FGzHHKXhz3qsE2Ddr4f5E75NRB1SXNdGerMSvLPsXReCxt3Z6VGg81l2WoJIkXpYcuS56dcaMyvwvahQVJBl4B4Q3pcSALBwx5qYMsLzpCt/H/pMywhFJTlcc86CplNBiioAQ8=:2017/12/14 08-38-30:9999920171214063830505,142";

            bool called = false;
            peerManager.OnUserquake += (s, e) =>
            {
                called = true;
                Assert.AreEqual("142", e.AreaCode);
            };
            invokeRaiseDataEvent(retreive);

            Assert.IsTrue(called);
        }
    }
}
