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

            PeerManager peerManager = new PeerManager();
            bool called = false;
            peerManager.OnAreapeers += (s, e) =>
            {
                Assert.AreEqual(expected, e.AreaPeerDictionary);
                called = true;
            };

            var type = peerManager.GetType();
            type.InvokeMember(
                "raiseDataEvent",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.InvokeMethod,
                null,
                peerManager,
                new object[] { Packet.Parse(retreive) }
                );

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
        public void raiseDataEvent_TsunamiData_RaiseOnTsunami()
        {
            Assert.Ignore("Not Implemented");
        }

        [TestCase]
        public void raiseDataEvent_UserquakeData_VerificationSucceeded()
        {
            Assert.Ignore("Not Implemented");
        }

        [TestCase]
        public void raiseDataEvent_UserquakeData_RaiseOnUserquake()
        {
            Assert.Ignore("Not Implemented");
        }
    }
}
