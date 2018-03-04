using System;
using System.Collections.Generic;
using System.IO;
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
            peerManager.ProtocolTime += () => { return DateTime.Now; };
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
            var retreive =
                "561 10 fLLhdQwhiy+wT9SaNwRIBIT9cAnD6DsupOpiVUXeK0CIoBg8Fo83q/xrIDGxHSn7lRvn2F8XINdgALm1akthxHWXDwInFS9+BEut2DDnY5tOoh3HxZS1XTCJC+qif5ebY6UUCV/AIbJFTKiZYS9hlrymj23ZquXMiDR563Au+1g=:2017/12/13 22-08-08:010,61;015,8;025,2;030,6;035,8;040,1;045,3;050,6;055,10;060,2;065,13;070,7;075,2;100,24;105,18;106,1;110,8;111,6;115,37;120,32;125,62;130,23;135,16;140,5;141,2;142,18;143,6;150,35;151,11;152,8;200,38;205,76;210,7;215,45;220,8;225,49;230,38;231,197;232,5;240,22;241,154;242,10;250,667;255,1;270,275;275,67;300,3;301,18;302,22;310,8;315,8;325,12;330,5;335,1;340,5;345,13;350,18;351,15;355,10;405,32;410,6;411,42;415,54;416,30;420,26;425,115;430,28;435,3;440,4;445,11;450,2;455,27;460,88;465,34;470,4;475,68;480,19;490,13;495,4;500,2;505,5;510,9;515,2;520,2;525,29;530,2;535,21;541,7;545,5;550,8;555,1;560,27;570,6;575,8;576,3;580,2;581,11;600,35;601,12;602,1;605,4;610,1;615,2;620,1;625,5;641,14;646,2;650,3;651,8;656,1;660,3;665,7;670,13;675,4;685,1;700,1;701,3;900,101;901,5;905,4";

            bool called = false;
            peerManager.OnAreapeers += (s, e) =>
            {
                Assert.IsTrue(e.IsValid);
                Assert.IsFalse(e.IsExpired);
                Assert.IsFalse(e.IsInvalidSignature);
                called = true;
            };
            peerManager.ProtocolTime += () => { return DateTime.Parse("2017/12/13 22:08:08"); };
            invokeRaiseDataEvent(retreive);

            Assert.IsTrue(called);
        }

        [TestCase]
        public void raiseDataEvent_AreapeersData_VerificationExpired()
        {
            var retreive =
                "561 10 fLLhdQwhiy+wT9SaNwRIBIT9cAnD6DsupOpiVUXeK0CIoBg8Fo83q/xrIDGxHSn7lRvn2F8XINdgALm1akthxHWXDwInFS9+BEut2DDnY5tOoh3HxZS1XTCJC+qif5ebY6UUCV/AIbJFTKiZYS9hlrymj23ZquXMiDR563Au+1g=:2017/12/13 22-08-08:010,61;015,8;025,2;030,6;035,8;040,1;045,3;050,6;055,10;060,2;065,13;070,7;075,2;100,24;105,18;106,1;110,8;111,6;115,37;120,32;125,62;130,23;135,16;140,5;141,2;142,18;143,6;150,35;151,11;152,8;200,38;205,76;210,7;215,45;220,8;225,49;230,38;231,197;232,5;240,22;241,154;242,10;250,667;255,1;270,275;275,67;300,3;301,18;302,22;310,8;315,8;325,12;330,5;335,1;340,5;345,13;350,18;351,15;355,10;405,32;410,6;411,42;415,54;416,30;420,26;425,115;430,28;435,3;440,4;445,11;450,2;455,27;460,88;465,34;470,4;475,68;480,19;490,13;495,4;500,2;505,5;510,9;515,2;520,2;525,29;530,2;535,21;541,7;545,5;550,8;555,1;560,27;570,6;575,8;576,3;580,2;581,11;600,35;601,12;602,1;605,4;610,1;615,2;620,1;625,5;641,14;646,2;650,3;651,8;656,1;660,3;665,7;670,13;675,4;685,1;700,1;701,3;900,101;901,5;905,4";

            bool called = false;
            peerManager.OnAreapeers += (s, e) =>
            {
                Assert.IsFalse(e.IsValid);
                Assert.IsTrue(e.IsExpired);
                Assert.IsFalse(e.IsInvalidSignature);
                called = true;
            };
            peerManager.ProtocolTime += () => { return DateTime.Parse("2017/12/13 22:08:09"); };
            invokeRaiseDataEvent(retreive);

            Assert.IsTrue(called);
        }

        [TestCase]
        public void raiseDataEvent_AreapeersData_VerificationInvalid()
        {
            var retreive =
                "561 10 fLLhdQwhiy+wT9SaNwRIBIT9cAnD6DsupOpiVUXeK0CIoBg8Fo83q/xrIDGxHSn7lRvn2F8XINdgALm1akthxHWXDwInFS9+BEut2DDnY5tOoh3HxZS1XTCJC+qif5ebY6UUCV/AIbJFTKiZYS9hlrymj23ZquXMiDR563Au+1g=:2017/12/13 22-08-08:010,61;015,8;025,2;030,6;035,8;040,1;045,3;050,6;055,10;060,2;065,13;070,7;075,2;100,24;105,18;106,1;110,8;111,6;115,37;120,32;125,62;130,23;135,16;140,5;141,2;142,18;143,6;150,35;151,11;152,8;200,38;205,76;210,7;215,45;220,8;225,49;230,38;231,197;232,5;240,22;241,154;242,10;250,667;255,1;270,275;275,67;300,3;301,18;302,22;310,8;315,8;325,12;330,5;335,1;340,5;345,13;350,18;351,15;355,10;405,32;410,6;411,42;415,54;416,30;420,26;425,115;430,28;435,3;440,4;445,11;450,2;455,27;460,88;465,34;470,4;475,68;480,19;490,13;495,4;500,2;505,5;510,9;515,2;520,2;525,29;530,2;535,21;541,7;545,5;550,8;555,1;560,27;570,6;575,8;576,3;580,2;581,11;600,35;601,12;602,1;605,4;610,1;615,2;620,1;625,5;641,14;646,2;650,3;651,8;656,1;660,3;665,7;670,13;675,4;685,1;700,1;701,3;900,101;901,5;905,3";

            bool called = false;
            peerManager.OnAreapeers += (s, e) =>
            {
                Assert.IsFalse(e.IsValid);
                Assert.IsFalse(e.IsExpired);
                Assert.IsTrue(e.IsInvalidSignature);
                called = true;
            };
            peerManager.ProtocolTime += () => { return DateTime.Parse("2017/12/13 22:08:08"); };
            invokeRaiseDataEvent(retreive);

            Assert.IsTrue(called);
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
        public void raiseDataEvent_EEWData_VerificationSucceeded()
        {
            var retreive =
                "561 12 S7PIyxJ9McMRmZsumTWZNVJGoh8sEsSxy0mc9nPpuI5ejNDjSJ7lhDpAZ6M+Hkj7c1kNlwMfFWfRr+7nx7j5wPuhAVFDmc40gT4d7Iap32auF3YFOv07PrWtFk5W1MwYc+HbJin6HDt64oZJS4jfYrFzdFPyKFmDINxQBuzouDg=:2018/03/01 22-45-51:010,67;015,7;025,1;030,4;035,9;040,1;045,4;050,5;055,10;060,1;065,8;070,7;075,2;100,27;105,27;106,1;110,8;111,3;115,33;120,32;125,57;130,22;135,11;140,6;141,2;142,14;143,5;150,35;151,8;152,7;200,38;205,51;210,4;215,51;220,9;225,50;230,36;231,202;232,4;240,18;241,145;242,8;250,641;255,1;270,253;275,60;300,3;301,9;302,12;305,1;310,8;315,5;325,20;330,3;335,2;340,3;345,9;350,21;351,17;355,12;400,2;405,40;410,2;411,43;415,36;416,22;420,26;425,96;430,25;435,3;440,2;445,7;455,34;460,79;465,41;470,3;475,56;480,15;490,13;495,3;500,3;505,7;510,8;515,1;520,3;525,19;530,4;535,22;541,3;545,4;550,7;555,2;560,20;570,8;575,11;576,4;580,1;581,11;600,27;601,12;602,3;605,6;610,1;615,3;620,2;625,6;641,10;646,2;650,4;651,7;656,2;660,1;665,4;670,9;675,5;685,1;700,1;701,4;900,102;901,6;905,5;950,0";

            bool called = false;
            peerManager.OnAreapeers += (s, e) => { };
            peerManager.OnEEWTest += (s, e) =>
            {
                called = true;
                Assert.IsTrue(e.IsValid);
                Assert.IsFalse(e.IsExpired);
                Assert.IsFalse(e.IsInvalidSignature);
            };
            peerManager.ProtocolTime += () => { return DateTime.Parse("2018/03/01 22:45:51"); };
            invokeRaiseDataEvent(retreive);

            Assert.IsTrue(called);
        }

        [TestCase]
        public void raiseDataEvent_EEWData_VerificationExpired()
        {
            var retreive =
                "561 12 S7PIyxJ9McMRmZsumTWZNVJGoh8sEsSxy0mc9nPpuI5ejNDjSJ7lhDpAZ6M+Hkj7c1kNlwMfFWfRr+7nx7j5wPuhAVFDmc40gT4d7Iap32auF3YFOv07PrWtFk5W1MwYc+HbJin6HDt64oZJS4jfYrFzdFPyKFmDINxQBuzouDg=:2018/03/01 22-45-51:010,67;015,7;025,1;030,4;035,9;040,1;045,4;050,5;055,10;060,1;065,8;070,7;075,2;100,27;105,27;106,1;110,8;111,3;115,33;120,32;125,57;130,22;135,11;140,6;141,2;142,14;143,5;150,35;151,8;152,7;200,38;205,51;210,4;215,51;220,9;225,50;230,36;231,202;232,4;240,18;241,145;242,8;250,641;255,1;270,253;275,60;300,3;301,9;302,12;305,1;310,8;315,5;325,20;330,3;335,2;340,3;345,9;350,21;351,17;355,12;400,2;405,40;410,2;411,43;415,36;416,22;420,26;425,96;430,25;435,3;440,2;445,7;455,34;460,79;465,41;470,3;475,56;480,15;490,13;495,3;500,3;505,7;510,8;515,1;520,3;525,19;530,4;535,22;541,3;545,4;550,7;555,2;560,20;570,8;575,11;576,4;580,1;581,11;600,27;601,12;602,3;605,6;610,1;615,3;620,2;625,6;641,10;646,2;650,4;651,7;656,2;660,1;665,4;670,9;675,5;685,1;700,1;701,4;900,102;901,6;905,5;950,0";

            bool called = false;
            peerManager.OnAreapeers += (s, e) => { };
            peerManager.OnEEWTest += (s, e) =>
            {
                called = true;
                Assert.IsFalse(e.IsValid);
                Assert.IsTrue(e.IsExpired);
                Assert.IsFalse(e.IsInvalidSignature);
            };
            peerManager.ProtocolTime += () => { return DateTime.Parse("2018/03/01 22:45:52"); };
            invokeRaiseDataEvent(retreive);

            Assert.IsTrue(called);
        }

        [TestCase]
        public void raiseDataEvent_EEWData_VerificationInvalid()
        {
            var retreive =
                "561 12 s7PIyxJ9McMRmZsumTWZNVJGoh8sEsSxy0mc9nPpuI5ejNDjSJ7lhDpAZ6M+Hkj7c1kNlwMfFWfRr+7nx7j5wPuhAVFDmc40gT4d7Iap32auF3YFOv07PrWtFk5W1MwYc+HbJin6HDt64oZJS4jfYrFzdFPyKFmDINxQBuzouDg=:2018/03/01 22-45-51:010,67;015,7;025,1;030,4;035,9;040,1;045,4;050,5;055,10;060,1;065,8;070,7;075,2;100,27;105,27;106,1;110,8;111,3;115,33;120,32;125,57;130,22;135,11;140,6;141,2;142,14;143,5;150,35;151,8;152,7;200,38;205,51;210,4;215,51;220,9;225,50;230,36;231,202;232,4;240,18;241,145;242,8;250,641;255,1;270,253;275,60;300,3;301,9;302,12;305,1;310,8;315,5;325,20;330,3;335,2;340,3;345,9;350,21;351,17;355,12;400,2;405,40;410,2;411,43;415,36;416,22;420,26;425,96;430,25;435,3;440,2;445,7;455,34;460,79;465,41;470,3;475,56;480,15;490,13;495,3;500,3;505,7;510,8;515,1;520,3;525,19;530,4;535,22;541,3;545,4;550,7;555,2;560,20;570,8;575,11;576,4;580,1;581,11;600,27;601,12;602,3;605,6;610,1;615,3;620,2;625,6;641,10;646,2;650,4;651,7;656,2;660,1;665,4;670,9;675,5;685,1;700,1;701,4;900,102;901,6;905,5;950,0";

            bool called = false;
            peerManager.OnAreapeers += (s, e) => { };
            peerManager.OnEEWTest += (s, e) =>
            {
                called = true;
                Assert.IsFalse(e.IsValid);
                Assert.IsFalse(e.IsExpired);
                Assert.IsTrue(e.IsInvalidSignature);
            };
            peerManager.ProtocolTime += () => { return DateTime.Parse("2018/03/01 22:45:51"); };
            invokeRaiseDataEvent(retreive);

            Assert.IsTrue(called);
        }

        [TestCase]
        public void raiseDataEvent_EEWData_RaiseOnEEWTest()
        {
            var retreive =
                "561 12 S7PIyxJ9McMRmZsumTWZNVJGoh8sEsSxy0mc9nPpuI5ejNDjSJ7lhDpAZ6M+Hkj7c1kNlwMfFWfRr+7nx7j5wPuhAVFDmc40gT4d7Iap32auF3YFOv07PrWtFk5W1MwYc+HbJin6HDt64oZJS4jfYrFzdFPyKFmDINxQBuzouDg=:2018/03/01 22-45-51:010,67;015,7;025,1;030,4;035,9;040,1;045,4;050,5;055,10;060,1;065,8;070,7;075,2;100,27;105,27;106,1;110,8;111,3;115,33;120,32;125,57;130,22;135,11;140,6;141,2;142,14;143,5;150,35;151,8;152,7;200,38;205,51;210,4;215,51;220,9;225,50;230,36;231,202;232,4;240,18;241,145;242,8;250,641;255,1;270,253;275,60;300,3;301,9;302,12;305,1;310,8;315,5;325,20;330,3;335,2;340,3;345,9;350,21;351,17;355,12;400,2;405,40;410,2;411,43;415,36;416,22;420,26;425,96;430,25;435,3;440,2;445,7;455,34;460,79;465,41;470,3;475,56;480,15;490,13;495,3;500,3;505,7;510,8;515,1;520,3;525,19;530,4;535,22;541,3;545,4;550,7;555,2;560,20;570,8;575,11;576,4;580,1;581,11;600,27;601,12;602,3;605,6;610,1;615,3;620,2;625,6;641,10;646,2;650,4;651,7;656,2;660,1;665,4;670,9;675,5;685,1;700,1;701,4;900,102;901,6;905,5;950,0";

            bool called = false;
            peerManager.OnAreapeers += (s, e) => { };
            peerManager.OnEEWTest += (s, e) =>
            {
                called = true;
                Assert.IsFalse(e.IsTest);
            };
            peerManager.ProtocolTime += () => { return DateTime.Parse("2018/03/01 22:45:51"); };
            invokeRaiseDataEvent(retreive);

            Assert.IsTrue(called);
        }

        [TestCase]
        public void raiseDataEvent_EarthquakeData_VerificationSucceeded()
        {
            var retreive =
                "551 7 sDQ5pmqYKrDT67bTjuYGR/7bKtrsm+OOanNYLmI25wAKBuJ3L0WIRrVcnDvE160d4pMvh0FRyVUp+EQeiBMoGsY291I2Qghq7S1z3+dOaPnYFqiwRc0crn1ZnL0ggKQU9vDHVEKgKE5xGcV7UQbOv01unGNP3gKgvqA+quDPWog=:2017/12/16 06-04-20:16日05時57分,3以上,0,2,大阪湾,10km,3.6,0,N34.4,E134.9,:";

            bool called = false;
            peerManager.OnEarthquake += (s, e) =>
            {
                called = true;
                Assert.IsTrue(e.IsValid);
                Assert.IsFalse(e.IsExpired);
                Assert.IsFalse(e.IsInvalidSignature);
            };
            peerManager.ProtocolTime += () => { return DateTime.Parse("2017/12/16 06:04:20"); };
            invokeRaiseDataEvent(retreive);

            Assert.IsTrue(called);
        }

        [TestCase]
        public void raiseDataEvent_EarthquakeData_VerificationExpired()
        {
            var retreive =
                "551 7 sDQ5pmqYKrDT67bTjuYGR/7bKtrsm+OOanNYLmI25wAKBuJ3L0WIRrVcnDvE160d4pMvh0FRyVUp+EQeiBMoGsY291I2Qghq7S1z3+dOaPnYFqiwRc0crn1ZnL0ggKQU9vDHVEKgKE5xGcV7UQbOv01unGNP3gKgvqA+quDPWog=:2017/12/16 06-04-20:16日05時57分,3以上,0,2,大阪湾,10km,3.6,0,N34.4,E134.9,:";

            bool called = false;
            peerManager.OnEarthquake += (s, e) =>
            {
                called = true;
                Assert.IsFalse(e.IsValid);
                Assert.IsTrue(e.IsExpired);
                Assert.IsFalse(e.IsInvalidSignature);
            };
            peerManager.ProtocolTime += () => { return DateTime.Parse("2017/12/16 06:04:21"); };
            invokeRaiseDataEvent(retreive);

            Assert.IsTrue(called);
        }

        [TestCase]
        public void raiseDataEvent_EarthquakeData_VerificationInvalid()
        {
            var retreive =
                "551 7 sDQ5pmqYKrDT67bTjuYGR/7bKtrsm+OOanNYLmI25wAKBuJ3L0WIRrVcnDvE160d4pMvh0FRyVUp+EQeiBMoGsY291I2Qghq7S1z3+dOaPnYFqiwRc0crn1ZnL0ggKQU9vDHVEKgKE5xGcV7UQbOv01unGNP3gKgvqA+quDPWog=:2017/12/16 06-04-20:16日05時57分,3以上,0,2,大阪湾,20km,3.6,0,N34.4,E134.9,:";

            bool called = false;
            peerManager.OnEarthquake += (s, e) =>
            {
                called = true;
                Assert.IsFalse(e.IsValid);
                Assert.IsFalse(e.IsExpired);
                Assert.IsTrue(e.IsInvalidSignature);
            };
            peerManager.ProtocolTime += () => { return DateTime.Parse("2017/12/16 06:04:20"); };
            invokeRaiseDataEvent(retreive);

            Assert.IsTrue(called);
        }

        [TestCase]
        public void raiseDataEvent_EarthquakeData_RaiseOnEarthquake_ScalePrompt_Hokkaido()
        {
            var retreive =
                "551 9 jMtE7OdLlOhBFTmE4JuWMXMan+GNBtV+69H5VHfP0JhRf/RqFro0Kz7KuTy6rlpBXQYiKgF3SC0bjIqkVVDBAeLOrXWm1IXm8/tNU9+ZiImWfSOSlI/bbhwyaajke2xdHlhLqJNzA7mpnyyDCc7GLL27LZ06XWKkf3jOmYUDutY=:2015/01/09 03-48-17:09日03時42分,4,2,1,,ごく浅い,-1.0,0,,,気象庁:+4,*釧路地方中南部,*根室地方中部,*根室地方南部,+3,*十勝地方中部,*十勝地方南部,*根室地方北部";

            bool called = false;
            peerManager.OnEarthquake += (s, e) =>
            {
                called = true;
                Assert.AreEqual(QuakeInformationType.ScalePrompt, e.InformationType);
                Assert.AreEqual(false, e.IsCorrection);
                Assert.AreEqual("09日03時42分", e.OccuredTime);
                Assert.AreEqual("4", e.Scale);
                Assert.AreEqual(DomesticTsunamiType.Checking, e.TsunamiType);

                object[,] expected =
                {
                    { null, "4", "釧路地方中南部" },
                    { null, "4", "根室地方中部" },
                    { null, "4", "根室地方南部" },
                    { null, "3", "十勝地方中部" },
                    { null, "3", "十勝地方南部" },
                    { null, "3", "根室地方北部" },
                };

                Assert.AreEqual(expected.GetLength(0), e.PointList.Count);

                for (int i = 0; i < expected.GetLength(0); i++)
                {
                    Assert.AreEqual(expected[i, 0], e.PointList[i].Prefecture);
                    Assert.AreEqual(expected[i, 1], e.PointList[i].Scale);
                    Assert.AreEqual(expected[i, 2], e.PointList[i].Name);
                }
            };
            invokeRaiseDataEvent(retreive);

            Assert.IsTrue(called);
        }

        [TestCase]
        public void raiseDataEvent_EarthquakeData_RaiseOnEarthquake_ScalePrompt_Other()
        {
            var retreive =
                "551 6 vI6WiN7W+iIa6r8a6pSrgOKJVjxWlssqRNWXiZKRfEjHAFciQwpS0PRPui4UqujClppPGHeiJ/dsrHa3sOhnyvZaFiiF0yI5UmjHksSYBmzSIE9RwgOzXwf9pbf6+roRr3bTM8SrRordXu5SFniqOIb4vlnMFwuLzJ8qy5V9n3Y=:2015/01/08 12-08-45:08日12時02分,3,2,1,,ごく浅い,-1.0,0,,,気象庁:-静岡県,+3,*静岡県西部,-愛知県,+3,*愛知県東部";

            bool called = false;
            peerManager.OnEarthquake += (s, e) =>
            {
                called = true;
                Assert.AreEqual(QuakeInformationType.ScalePrompt, e.InformationType);
                Assert.AreEqual(false, e.IsCorrection);
                Assert.AreEqual("08日12時02分", e.OccuredTime);
                Assert.AreEqual("3", e.Scale);
                Assert.AreEqual(DomesticTsunamiType.Checking, e.TsunamiType);

                object[,] expected =
                {
                    { "静岡県", "3", "静岡県西部" },
                    { "愛知県", "3", "愛知県東部" },
                };

                Assert.AreEqual(expected.GetLength(0), e.PointList.Count);

                for (int i = 0; i < expected.GetLength(0); i++)
                {
                    Assert.AreEqual(expected[i, 0], e.PointList[i].Prefecture);
                    Assert.AreEqual(expected[i, 1], e.PointList[i].Scale);
                    Assert.AreEqual(expected[i, 2], e.PointList[i].Name);
                }
            };
            invokeRaiseDataEvent(retreive);

            Assert.IsTrue(called);
        }

        [TestCase]
        public void raiseDataEvent_EarthquakeData_RaiseOnEarthquake_Destination()
        {
            var retreive =
                "551 7 sDQ5pmqYKrDT67bTjuYGR/7bKtrsm+OOanNYLmI25wAKBuJ3L0WIRrVcnDvE160d4pMvh0FRyVUp+EQeiBMoGsY291I2Qghq7S1z3+dOaPnYFqiwRc0crn1ZnL0ggKQU9vDHVEKgKE5xGcV7UQbOv01unGNP3gKgvqA+quDPWog=:2017/12/16 06-04-20:16日05時57分,3以上,0,2,大阪湾,10km,3.6,0,N34.4,E134.9,:";

            bool called = false;
            peerManager.OnEarthquake += (s, e) =>
            {
                called = true;
                Assert.AreEqual(QuakeInformationType.Destination, e.InformationType);
                Assert.AreEqual("10km", e.Depth);
                Assert.AreEqual("大阪湾", e.Destination);
                Assert.AreEqual(false, e.IsCorrection);
                Assert.AreEqual("N34.4", e.Latitude);
                Assert.AreEqual("E134.9", e.Longitude);
                Assert.AreEqual("3.6", e.Magnitude);
                Assert.AreEqual("16日05時57分", e.OccuredTime);
                Assert.AreEqual(DomesticTsunamiType.None, e.TsunamiType);

                Assert.IsTrue(e.PointList == null || e.PointList.Count == 0);
            };
            invokeRaiseDataEvent(retreive);

            Assert.IsTrue(called);
        }

        [TestCase]
        public void raiseDataEvent_EarthquakeData_RaiseOnEarthquake_ScaleAndDestination()
        {
            var retreive =
                "551 7 EgwaB8hT7yMf52JNYWpSwxn345g83P9nO/ImHq9Y5XidD4BqaNEogt17edT+IAO6haST96Rnef+2B6tKKEesDDxu3hx+xoYkSHZGw1Ln7GM9Eb2tfsnLBHJE1QrGtfLEoB4uNoEQsI8C5z9ykjjf8RoYXA1Xo4ul+ZXvGQxFsPE=:2015/01/23 18-19-44:23日18時11分,3,0,3,福島県沖,40km,4.2,0,N37.1,E141.1,気象庁:-福島県,+3,*福島県中通り";

            bool called = false;
            peerManager.OnEarthquake += (s, e) =>
            {
                called = true;
                Assert.AreEqual(QuakeInformationType.ScaleAndDestination, e.InformationType);
                Assert.AreEqual("40km", e.Depth);
                Assert.AreEqual("福島県沖", e.Destination);
                Assert.AreEqual(false, e.IsCorrection);
                Assert.AreEqual("N37.1", e.Latitude);
                Assert.AreEqual("E141.1", e.Longitude);
                Assert.AreEqual("4.2", e.Magnitude);
                Assert.AreEqual("23日18時11分", e.OccuredTime);
                Assert.AreEqual("3", e.Scale);
                Assert.AreEqual(DomesticTsunamiType.None, e.TsunamiType);

                object[,] expected =
                {
                    { "福島県", "3", "福島県中通り" },
                };

                Assert.AreEqual(expected.GetLength(0), e.PointList.Count);

                for (int i = 0; i < expected.GetLength(0); i++)
                {
                    Assert.AreEqual(expected[i, 0], e.PointList[i].Prefecture);
                    Assert.AreEqual(expected[i, 1], e.PointList[i].Scale);
                    Assert.AreEqual(expected[i, 2], e.PointList[i].Name);
                }
            };
            invokeRaiseDataEvent(retreive);

            Assert.IsTrue(called);
        }

        [TestCase]
        public void raiseDataEvent_EarthquakeData_RaiseOnEarthquake_Detail()
        {
            var retreive =
                "551 5 H6p1hBYECBGllLEBh7f08mu7d5s4LzGQLJfFpXPjaTvRWs/vxJAPuKeO5FT00m4m5UuNIbmIsuGemAPzNeNlKxeuPsfFMFahGN0AVW95SzhLIrkHyJZkbuI0gf/3V9ciadfDEmw0qICfrUw+1JB1dXydgjxt40Qcg9TpN2JS3ks=:2014/08/15 12-28-24:15日12時20分,2,0,4,青森県東方沖,60km,4.2,0,N41.5,E142.6,:-北海道,+2,*函館市,+1,*千歳市,*新ひだか町,*浦河町,*様似町,*えりも町,-青森県,+2,*東通村,+1,*外ヶ浜町,*八戸市,*三沢市,*野辺地町,*七戸町,*東北町,*六ヶ所村,*五戸町,*青森南部町,*階上町,*むつ市,-岩手県,+1,*軽米町";

            bool called = false;
            peerManager.OnEarthquake += (s, e) =>
            {
                called = true;
                Assert.AreEqual(QuakeInformationType.Detail, e.InformationType);
                Assert.AreEqual("60km", e.Depth);
                Assert.AreEqual("青森県東方沖", e.Destination);
                Assert.AreEqual(false, e.IsCorrection);
                Assert.AreEqual("N41.5", e.Latitude);
                Assert.AreEqual("E142.6", e.Longitude);
                Assert.AreEqual("4.2", e.Magnitude);
                Assert.AreEqual("15日12時20分", e.OccuredTime);
                Assert.AreEqual("2", e.Scale);
                Assert.AreEqual(DomesticTsunamiType.None, e.TsunamiType);

                object[,] expected =
                {
                    { "北海道", "2", "函館市" },
                    { "北海道", "1", "千歳市" },
                    { "北海道", "1", "新ひだか町" },
                    { "北海道", "1", "浦河町" },
                    { "北海道", "1", "様似町" },
                    { "北海道", "1", "えりも町" },
                    { "青森県", "2", "東通村" },
                    { "青森県", "1", "外ヶ浜町" },
                    { "青森県", "1", "八戸市" },
                    { "青森県", "1", "三沢市" },
                    { "青森県", "1", "野辺地町" },
                    { "青森県", "1", "七戸町" },
                    { "青森県", "1", "東北町" },
                    { "青森県", "1", "六ヶ所村" },
                    { "青森県", "1", "五戸町" },
                    { "青森県", "1", "青森南部町" },
                    { "青森県", "1", "階上町" },
                    { "青森県", "1", "むつ市" },
                    { "岩手県", "1", "軽米町" },
                };

                Assert.AreEqual(expected.GetLength(0), e.PointList.Count);

                for (int i = 0; i < expected.GetLength(0); i++)
                {
                    Assert.AreEqual(expected[i, 0], e.PointList[i].Prefecture);
                    Assert.AreEqual(expected[i, 1], e.PointList[i].Scale);
                    Assert.AreEqual(expected[i, 2], e.PointList[i].Name);
                }
            };
            invokeRaiseDataEvent(retreive);

            Assert.IsTrue(called);
        }

        [TestCase]
        public void raiseDataEvent_EarthquakeData_RaiseOnEarthquake_Detail_Correction()
        {
            var retreive =
                "551 12 Z9gtkHsA/oykJ7JRBdg6ITlc+9CtMViY+9fZ5pDlB4ZiVGFu6agcnqJx4s7SVpA6/tHL0P6AEER4eLzAyFq1I3yjx9HvOET3cU05RJR2ywglQ1WIOxl75zOTXKJpexquNblGy1clOfUyBX6AnPh420fSL/XmLt68HJeP7TfwvNU=:2014/10/15 08-07-08:15日07時52分,3,0,4,沖縄本島近海,50km,5.1,1,N26.3,E127.4,:-沖縄県,+3,*粟国村,*渡嘉敷村,*渡名喜村,*八重瀬町,*久米島町,+2,*名護市,*国頭村,*今帰仁村,*恩納村,*宜野座村,*金武町,*那覇市,*那覇空港,*宜野湾市,*浦添市,*糸満市,*沖縄市,*読谷村,*北谷町,*北中城村,*中城村,*西原町,*豊見城市,*与那原町,*南風原町,*うるま市,*南城市,+1,*東村,*伊江村,*嘉手納町,-鹿児島県,+1,*与論町";

            bool called = false;
            peerManager.OnEarthquake += (s, e) =>
            {
                called = true;
                Assert.AreEqual(QuakeInformationType.Detail, e.InformationType);
                Assert.AreEqual("50km", e.Depth);
                Assert.AreEqual("沖縄本島近海", e.Destination);
                Assert.AreEqual(true, e.IsCorrection);
                Assert.AreEqual("N26.3", e.Latitude);
                Assert.AreEqual("E127.4", e.Longitude);
                Assert.AreEqual("5.1", e.Magnitude);
                Assert.AreEqual("15日07時52分", e.OccuredTime);
                Assert.AreEqual("3", e.Scale);
                Assert.AreEqual(DomesticTsunamiType.None, e.TsunamiType);
            };
            invokeRaiseDataEvent(retreive);

            Assert.IsTrue(called);
        }

        [TestCase]
        public void raiseDataEvent_EarthquakeData_RaiseOnEarthquake_Detail_Tsunami()
        {
            var retreive =
                "551 6 MGjYR67hSGJ/Lv50PE9FmcG7YcPtyU8iqn+wId4v+ci4eW3EjQFx80yukGzK8LGD3GabIebDbgKFZuaoJcP5+3qeQyGmRrIpA3m5m670jqirODhrYe+MECq8erTQFkKnHizcZAYjk5M2dfgpfC5y0IT+tEZ3igfLJZTzrIZLwXc=:2015/04/20 11-01-34:20日10時49分,1,1,4,与那国島近海,20km,5.2,0,N24.3,E122.4,:-沖縄県,+1,*与那国町";

            bool called = false;
            peerManager.OnEarthquake += (s, e) =>
            {
                called = true;
                Assert.AreEqual(QuakeInformationType.Detail, e.InformationType);
                Assert.AreEqual("20km", e.Depth);
                Assert.AreEqual("与那国島近海", e.Destination);
                Assert.AreEqual(false, e.IsCorrection);
                Assert.AreEqual("N24.3", e.Latitude);
                Assert.AreEqual("E122.4", e.Longitude);
                Assert.AreEqual("5.2", e.Magnitude);
                Assert.AreEqual("20日10時49分", e.OccuredTime);
                Assert.AreEqual("1", e.Scale);
                Assert.AreEqual(DomesticTsunamiType.Effective, e.TsunamiType);

                object[,] expected =
                {
                    { "沖縄県", "1", "与那国町" },
                };

                Assert.AreEqual(expected.GetLength(0), e.PointList.Count);

                for (int i = 0; i < expected.GetLength(0); i++)
                {
                    Assert.AreEqual(expected[i, 0], e.PointList[i].Prefecture);
                    Assert.AreEqual(expected[i, 1], e.PointList[i].Scale);
                    Assert.AreEqual(expected[i, 2], e.PointList[i].Name);
                }
            };
            invokeRaiseDataEvent(retreive);

            Assert.IsTrue(called);
        }

        [TestCase]
        public void raiseDataEvent_EarthquakeData_RaiseOnEarthquake_Foreign()
        {
            var retreive =
                "551 7 od6C+LthyfJ1Ha+vjDks17ZeeBfJaC2M2dGr053sQ8tTT1Axx9QUkZNqLg7RTIIjkY3OLlxAzXMn3sdLKe1if8Isv+f8vGv0H5xWhGp7ATR3ehzCcDeGokPRShGyFuj/83+2Iz+Tb/yAoWz224Snbjx7OX1zOfnzrN92oSiA97I=:2015/05/05 11-55-46:05日10時44分,0,0,5,パプアニューギニア、ニューブリテン,ごく浅い,7.5,0,S5.6,E152.1,気象庁:";

            bool called = false;
            peerManager.OnEarthquake += (s, e) =>
            {
                called = true;
                Assert.AreEqual(QuakeInformationType.Foreign, e.InformationType);
                Assert.AreEqual("ごく浅い", e.Depth);
                Assert.AreEqual("パプアニューギニア、ニューブリテン", e.Destination);
                Assert.AreEqual(false, e.IsCorrection);
                Assert.AreEqual("S5.6", e.Latitude);
                Assert.AreEqual("E152.1", e.Longitude);
                Assert.AreEqual("7.5", e.Magnitude);
                Assert.AreEqual("05日10時44分", e.OccuredTime);
                Assert.AreEqual(DomesticTsunamiType.None, e.TsunamiType);

                Assert.IsTrue(e.PointList == null || e.PointList.Count == 0);
            };
            invokeRaiseDataEvent(retreive);

            Assert.IsTrue(called);
        }

        [TestCase]
        public void raiseDataEvent_EarthquakeData_RaiseOnEarthquake_Other()
        {
            var retreive =
                "551 5 aFb8UWobsHdtSR2eW2zezd7kKXOIDt0L2eHqtJGXkcWCUmngFMQuNcof89QzFl+XaX4U+edN2AimCWdPVS2F7ANXkZjtf/XK0fLCrIPs2IcZbzIIC2cODwPNB245pjZr44iWWGbDnZqYiKcTXZgtuashyj0512Wz1Ceeq5TpP/w=:2015/02/17 09-56-36:01日00時00分,0,3,6,,ごく浅い,-1.0,1,,,気象庁:";

            bool called = false;
            peerManager.OnEarthquake += (s, e) =>
            {
                called = true;
                Assert.AreEqual(QuakeInformationType.Unknown, e.InformationType);
            };
            invokeRaiseDataEvent(retreive);

            Assert.IsTrue(called);
        }

        [TestCase]
        public void raiseDataEvent_TsunamiData_VerificationSucceeded()
        {
            var retreive =
                "552 8 CU2Au7WAvqoiKXaAfmEEXS2W4vvePShT5W6cXXSsTpSfd9OA23Oc0GulQRr5D+mJ2t9mnOATVml5F7jwlGOMugMSFR49cmhFSIlLg3eDisotDvKIsNXpr//T3vcF9UiZA5JJzzcaXZuWg1591CXBN/xkWJf4C73LE3HmQewtR0w=:2016/11/22 12-53-43:解除";

            bool called = false;
            peerManager.OnTsunami += (s, e) =>
            {
                called = true;
                Assert.IsTrue(e.IsValid);
                Assert.IsFalse(e.IsExpired);
                Assert.IsFalse(e.IsInvalidSignature);
            };
            peerManager.ProtocolTime += () => { return DateTime.Parse("2016/11/22 12:53:43"); };
            invokeRaiseDataEvent(retreive);

            Assert.IsTrue(called);
        }

        [TestCase]
        public void raiseDataEvent_TsunamiData_VerificationExpired()
        {
            var retreive =
                "552 8 CU2Au7WAvqoiKXaAfmEEXS2W4vvePShT5W6cXXSsTpSfd9OA23Oc0GulQRr5D+mJ2t9mnOATVml5F7jwlGOMugMSFR49cmhFSIlLg3eDisotDvKIsNXpr//T3vcF9UiZA5JJzzcaXZuWg1591CXBN/xkWJf4C73LE3HmQewtR0w=:2016/11/22 12-53-43:解除";

            bool called = false;
            peerManager.OnTsunami += (s, e) =>
            {
                called = true;
                Assert.IsFalse(e.IsValid);
                Assert.IsTrue(e.IsExpired);
                Assert.IsFalse(e.IsInvalidSignature);
            };
            peerManager.ProtocolTime += () => { return DateTime.Parse("2016/11/22 12:53:44"); };
            invokeRaiseDataEvent(retreive);

            Assert.IsTrue(called);
        }

        [TestCase]
        public void raiseDataEvent_TsunamiData_VerificationInvalid()
        {
            var retreive =
                "552 8 CU2Au7WAvqoiKXaAfmEEXS2W4vvePShT5W6cXXSsTpSfd9OA23Oc0GulQRr5D+mJ2t9mnOATVml5F7jwlGOMugMSFR49cmhFSIlLg3eDisotDvKIsNXpr//T3vcF9UiZA5JJzzcaXZuWg1591CXBN/xkWJf4C73LE3HmQewtR0w=:2016/11/22 12-53-43:解";

            bool called = false;
            peerManager.OnTsunami += (s, e) =>
            {
                called = true;
                Assert.IsFalse(e.IsValid);
                Assert.IsFalse(e.IsExpired);
                Assert.IsTrue(e.IsInvalidSignature);
            };
            peerManager.ProtocolTime += () => { return DateTime.Parse("2016/11/22 12:53:43"); };
            invokeRaiseDataEvent(retreive);

            Assert.IsTrue(called);
        }

        [TestCase]
        public void raiseDataEvent_TsunamiData_RaiseOnTsunami_MajorWarning()
        {
            // ダミーデータ
            var retreive =
                "552 11 Oqqn36REt0RjpKqBibXn6A1woRon/xQ7ySYVmET24n3gxaKAEBE/e+eyQ9TUAAePDEkyUc/pGB+S3w8vyHE6gYRuCNx9ysC7gsdSNh/Bv4C3T3p0WDv2GtJq1jSETkcZNkKv9p5Ptny5+kHt9y5dPA/enZQ7yPCul5EezHsxM80=:2016/11/22 07-30-21:-大津波警報,+青森県太平洋沿岸,+岩手県,+宮城県,-津波警報,+北海道太平洋沿岸東部,+北海道太平洋沿岸中部,+北海道太平洋沿岸西部,+青森県日本海沿岸,+福島県,+茨城県,+千葉県九十九里・外房,+千葉県内房,+東京湾内湾,+伊豆諸島,+小笠原諸島,+相模湾・三浦半島,+静岡県,+愛知県外海,+伊勢・三河湾,+三重県南部,+淡路島南部,+和歌山県,+岡山県,+徳島県,+愛媛県宇和海沿岸,+高知県,+有明・八代海,+大分県瀬戸内海沿岸,+大分県豊後水道沿岸,+宮崎県,+鹿児島県東部,+種子島・屋久島地方,+奄美諸島・トカラ列島,+鹿児島県西部,+沖縄本島地方,+大東島地方,+宮古島・八重山地方,-津波注意報,+北海道日本海沿岸南部,+オホーツク海沿岸,+陸奥湾,+大阪府,+兵庫県瀬戸内海沿岸,+広島県,+香川県,+愛媛県瀬戸内海沿岸,+山口県瀬戸内海沿岸,+福岡県瀬戸内海沿岸,+福岡県日本海沿岸,+長崎県西方,+熊本県天草灘沿岸";

            bool called = false;
            peerManager.OnTsunami += (s, e) =>
            {
                called = true;
                Assert.IsFalse(e.IsCancelled);

                object[,] expected =
                {
                    { TsunamiCategory.MajorWarning, false, "青森県太平洋沿岸" },
                    { TsunamiCategory.MajorWarning, false, "岩手県" },
                    { TsunamiCategory.MajorWarning, false, "宮城県" },
                    { TsunamiCategory.Warning, false, "北海道太平洋沿岸東部" },
                    { TsunamiCategory.Warning, false, "北海道太平洋沿岸中部" },
                    { TsunamiCategory.Warning, false, "北海道太平洋沿岸西部" },
                    { TsunamiCategory.Warning, false, "青森県日本海沿岸" },
                    { TsunamiCategory.Warning, false, "福島県" },
                    { TsunamiCategory.Warning, false, "茨城県" },
                    { TsunamiCategory.Warning, false, "千葉県九十九里・外房" },
                    { TsunamiCategory.Warning, false, "千葉県内房" },
                    { TsunamiCategory.Warning, false, "東京湾内湾" },
                    { TsunamiCategory.Warning, false, "伊豆諸島" },
                    { TsunamiCategory.Warning, false, "小笠原諸島" },
                    { TsunamiCategory.Warning, false, "相模湾・三浦半島" },
                    { TsunamiCategory.Warning, false, "静岡県" },
                    { TsunamiCategory.Warning, false, "愛知県外海" },
                    { TsunamiCategory.Warning, false, "伊勢・三河湾" },
                    { TsunamiCategory.Warning, false, "三重県南部" },
                    { TsunamiCategory.Warning, false, "淡路島南部" },
                    { TsunamiCategory.Warning, false, "和歌山県" },
                    { TsunamiCategory.Warning, false, "岡山県" },
                    { TsunamiCategory.Warning, false, "徳島県" },
                    { TsunamiCategory.Warning, false, "愛媛県宇和海沿岸" },
                    { TsunamiCategory.Warning, false, "高知県" },
                    { TsunamiCategory.Warning, false, "有明・八代海" },
                    { TsunamiCategory.Warning, false, "大分県瀬戸内海沿岸" },
                    { TsunamiCategory.Warning, false, "大分県豊後水道沿岸" },
                    { TsunamiCategory.Warning, false, "宮崎県" },
                    { TsunamiCategory.Warning, false, "鹿児島県東部" },
                    { TsunamiCategory.Warning, false, "種子島・屋久島地方" },
                    { TsunamiCategory.Warning, false, "奄美諸島・トカラ列島" },
                    { TsunamiCategory.Warning, false, "鹿児島県西部" },
                    { TsunamiCategory.Warning, false, "沖縄本島地方" },
                    { TsunamiCategory.Warning, false, "大東島地方" },
                    { TsunamiCategory.Warning, false, "宮古島・八重山地方" },
                    { TsunamiCategory.Advisory, false, "北海道日本海沿岸南部" },
                    { TsunamiCategory.Advisory, false, "オホーツク海沿岸" },
                    { TsunamiCategory.Advisory, false, "陸奥湾" },
                    { TsunamiCategory.Advisory, false, "大阪府" },
                    { TsunamiCategory.Advisory, false, "兵庫県瀬戸内海沿岸" },
                    { TsunamiCategory.Advisory, false, "広島県" },
                    { TsunamiCategory.Advisory, false, "香川県" },
                    { TsunamiCategory.Advisory, false, "愛媛県瀬戸内海沿岸" },
                    { TsunamiCategory.Advisory, false, "山口県瀬戸内海沿岸" },
                    { TsunamiCategory.Advisory, false, "福岡県瀬戸内海沿岸" },
                    { TsunamiCategory.Advisory, false, "福岡県日本海沿岸" },
                    { TsunamiCategory.Advisory, false, "長崎県西方" },
                    { TsunamiCategory.Advisory, false, "熊本県天草灘沿岸" },
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
            var retreive =
                "555 7 IyZUbE0C5de3CrUYIN+LqhsG6C3+E1dNoxD1Ef02qiri4qW7lqPb5J2p1Fhe3rZs:2017/12/14 06-39-30:MEwwDQYJKoZIhvcNAQEBBQADOwAwOAIxAKK/e5GQjXQPWQRhsMxTmOfQ9ISxDKb747F5g80s9wlB4XIjH7Ig8bycZp90vz3WzQIDAQAB:j74KAyS6Yy5LrEzm33OGBMMkhXT14n6bKajP4FGzHHKXhz3qsE2Ddr4f5E75NRB1SXNdGerMSvLPsXReCxt3Z6VGg81l2WoJIkXpYcuS56dcaMyvwvahQVJBl4B4Q3pcSALBwx5qYMsLzpCt/H/pMywhFJTlcc86CplNBiioAQ8=:2017/12/14 08-38-30:9999920171214063830505,142";

            bool called = false;
            peerManager.OnUserquake += (s, e) =>
            {
                called = true;
                Assert.IsTrue(e.IsValid);
                Assert.IsFalse(e.IsExpired);
                Assert.IsFalse(e.IsInvalidSignature);
            };
            peerManager.ProtocolTime += () => { return DateTime.Parse("2017/12/14 06:39:30"); };
            invokeRaiseDataEvent(retreive);

            Assert.IsTrue(called);
        }

        [TestCase]
        public void raiseDataEvent_UserquakeData_VerificationExpired()
        {
            var retreive =
                "555 7 IyZUbE0C5de3CrUYIN+LqhsG6C3+E1dNoxD1Ef02qiri4qW7lqPb5J2p1Fhe3rZs:2017/12/14 06-39-30:MEwwDQYJKoZIhvcNAQEBBQADOwAwOAIxAKK/e5GQjXQPWQRhsMxTmOfQ9ISxDKb747F5g80s9wlB4XIjH7Ig8bycZp90vz3WzQIDAQAB:j74KAyS6Yy5LrEzm33OGBMMkhXT14n6bKajP4FGzHHKXhz3qsE2Ddr4f5E75NRB1SXNdGerMSvLPsXReCxt3Z6VGg81l2WoJIkXpYcuS56dcaMyvwvahQVJBl4B4Q3pcSALBwx5qYMsLzpCt/H/pMywhFJTlcc86CplNBiioAQ8=:2017/12/14 08-38-30:9999920171214063830505,142";

            bool called = false;
            peerManager.OnUserquake += (s, e) =>
            {
                called = true;
                Assert.IsFalse(e.IsValid);
                Assert.IsTrue(e.IsExpired);
                Assert.IsFalse(e.IsInvalidSignature);
            };
            peerManager.ProtocolTime += () => { return DateTime.Parse("2017/12/14 06:39:31"); };
            invokeRaiseDataEvent(retreive);

            Assert.IsTrue(called);
        }

        [TestCase]
        public void raiseDataEvent_UserquakeData_VerificationInvalid()
        {
            var retreive =
                "555 7 IyZUbE0C5de3CrUYIN+LqhsG6C3+E1dNoxD1Ef02qiri4qW7lqPb5J2p1Fhe3rZs:2017/12/14 06-39-30:MEwwDQYJKoZIhvcNAQEBBQADOwAwOAIxAKK/e5GQjXQPWQRhsMxTmOfQ9ISxDKb747F5g80s9wlB4XIjH7Ig8bycZp90vz3WzQIDAQAB:j74KAyS6Yy5LrEzm33OGBMMkhXT14n6bKajP4FGzHHKXhz3qsE2Ddr4f5E75NRB1SXNdGerMSvLPsXReCxt3Z6VGg81l2WoJIkXpYcuS56dcaMyvwvahQVJBl4B4Q3pcSALBwx5qYMsLzpCt/H/pMywhFJTlcc86CplNBiioAQ8=:2017/12/14 08-38-30:9999920171214063830505,143";

            bool called = false;
            peerManager.OnUserquake += (s, e) =>
            {
                called = true;
                Assert.IsFalse(e.IsValid);
                Assert.IsFalse(e.IsExpired);
                Assert.IsTrue(e.IsInvalidSignature);
            };
            peerManager.ProtocolTime += () => { return DateTime.Parse("2017/12/14 06:39:30"); };
            invokeRaiseDataEvent(retreive);

            Assert.IsTrue(called);
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

        [TestCase]
        public void raiseDataEvent_Smoke()
        {
            var filename = @"TestData/Peer/Manager/PeerManagerRaiseDataEventTest_Smoke.txt";
            StreamReader reader = new StreamReader(filename, Encoding.UTF8);

            int lineCount = 0;
            int eventCount = 0;

            peerManager.OnAreapeers += (s, e) => { eventCount++; };
            peerManager.OnEarthquake += (s, e) => { eventCount++; };
            peerManager.OnTsunami += (s, e) => { eventCount++; };
            peerManager.OnUserquake += (s, e) => { eventCount++; };

            while (reader.Peek() >= 0)
            {
                lineCount++;
                string line = reader.ReadLine();
                invokeRaiseDataEvent(line);
            }

            reader.Close();

            Assert.AreEqual(lineCount, eventCount);
        }
    }
}
