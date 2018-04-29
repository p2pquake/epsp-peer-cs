using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Client.Peer;
using CUIClient.Handler;
using NUnit.Framework;

namespace CUIClientTest.Handler
{
    [TestFixture]
    class EPSPHandlerTest
    {
        private EPSPHandler epspHandler;
        private DateTime dateTime;
        private StringWriter stringWriter;

        [SetUp]
        public void SetUp()
        {
            epspHandler = new EPSPHandler(GetDateTime);
            stringWriter = new StringWriter();
            Console.SetOut(stringWriter);
        }

        [TearDown]
        public void TearDown()
        {
            stringWriter.Close();
        }

        [TestCase]
        public void AreaPeersTest()
        {
            var messageList = new List<string>();

            var areapeers = new EPSPAreapeersEventArgs();
            areapeers.IsInvalidSignature = false;
            areapeers.IsExpired = false;
            areapeers.AreaPeerDictionary = new Dictionary<string, int>();

            // 無効な署名データ
            areapeers.IsExpired = true;
            epspHandler.MediatorContext_OnAreapeers(this, areapeers);
            Assert.AreEqual(string.Join("\r\n", messageList), GetWriterOutput());

            // ゼロデータ
            areapeers.IsExpired = false;
            epspHandler.MediatorContext_OnAreapeers(this, areapeers);
            messageList.Add("地域ピア数の情報を受信しました: ピア数 0");
            Assert.AreEqual(string.Join("\r\n", messageList), GetWriterOutput());

            // 有効データ
            areapeers.AreaPeerDictionary = new Dictionary<string, int>() { { "100", 5 }, { "105", 3 } };
            epspHandler.MediatorContext_OnAreapeers(this, areapeers);
            messageList.Add("地域ピア数の情報を受信しました: ピア数 8");
            Assert.AreEqual(string.Join("\r\n", messageList), GetWriterOutput());
        }

        [TestCase]
        public void EarthquakeTest()
        {
            EPSPQuakeEventArgs quake;
            var messageList = new List<string>();

            // 無効な署名データ
            quake = new EPSPQuakeEventArgs();
            quake.IsInvalidSignature = false;
            quake.IsExpired = true;
            epspHandler.MediatorContext_OnEarthquake(this, quake);
            Assert.AreEqual(string.Join("\r\n", messageList), GetWriterOutput());

            // 共通データ
            quake = new EPSPQuakeEventArgs();
            quake.IsInvalidSignature = false;
            quake.IsExpired = false;
            quake.OccuredTime = "1日2時3分";
            quake.Scale = "5弱";
            quake.TsunamiType = DomesticTsunamiType.None;
            quake.Destination = "東京湾";
            quake.Magnitude = "5.8";
            quake.Depth = "10km";
            quake.PointList = new List<QuakeObservationPoint>()
            {
                new QuakeObservationPoint() { Name = "東京都２３区", Scale = "5弱" },
                new QuakeObservationPoint() { Name = "神奈川県東部", Scale = "4" },
                new QuakeObservationPoint() { Name = "東京都多摩東部", Scale = "3" },
            };

            // 震度速報
            quake.InformationType = QuakeInformationType.ScalePrompt;
            epspHandler.MediatorContext_OnEarthquake(this, quake);
            messageList.Add("地震情報を受信しました: 1日2時3分 震度5弱 ");
            Assert.AreEqual(string.Join("\r\n", messageList), GetWriterOutput());

            // 震源情報
            quake.InformationType = QuakeInformationType.Destination;
            epspHandler.MediatorContext_OnEarthquake(this, quake);
            messageList.Add("地震情報を受信しました: 1日2時3分 東京湾(10km, M5.8) 津波の心配なし");
            Assert.AreEqual(string.Join("\r\n", messageList), GetWriterOutput());

            // 震度・震源情報
            quake.InformationType = QuakeInformationType.ScaleAndDestination;
            epspHandler.MediatorContext_OnEarthquake(this, quake);
            messageList.Add("地震情報を受信しました: 1日2時3分 震度5弱 東京湾(10km, M5.8) 津波の心配なし");
            Assert.AreEqual(string.Join("\r\n", messageList), GetWriterOutput());

            // 各地の震度情報
            quake.InformationType = QuakeInformationType.Detail;
            epspHandler.MediatorContext_OnEarthquake(this, quake);
            messageList.Add("地震情報を受信しました: 1日2時3分 震度5弱 東京湾(10km, M5.8) 津波の心配なし");
            Assert.AreEqual(string.Join("\r\n", messageList), GetWriterOutput());

            // 遠地地震情報
            quake.InformationType = QuakeInformationType.Foreign;
            epspHandler.MediatorContext_OnEarthquake(this, quake);
            messageList.Add("地震情報を受信しました: 1日2時3分 東京湾(10km, M5.8) 津波の心配なし");
            Assert.AreEqual(string.Join("\r\n", messageList), GetWriterOutput());
        }

        [TestCase]
        public void EEWTest()
        {
            var messageList = new List<string>();

            var eew = new EPSPEEWTestEventArgs();
            eew.IsInvalidSignature = false;
            eew.IsExpired = false;
            eew.IsTest = true;
            
            // 試験報
            epspHandler.MediatorContext_OnEEWTest(this, eew);
            Assert.AreEqual(string.Join("\r\n", messageList), GetWriterOutput());
            
            // 無効な署名データ
            eew.IsTest = false;
            eew.IsInvalidSignature = true;
            epspHandler.MediatorContext_OnEEWTest(this, eew);
            Assert.AreEqual(string.Join("\r\n", messageList), GetWriterOutput());

            // 有効データ
            eew.IsInvalidSignature = false;
            epspHandler.MediatorContext_OnEEWTest(this, eew);
            messageList.Add("緊急地震速報 配信試験（オープンβ）の情報を受信しました。");
            Assert.AreEqual(string.Join("\r\n", messageList), GetWriterOutput());
        }

        [TestCase]
        public void TsunamiTest()
        {
            var messageList = new List<string>();
            
            // 無効な署名データ
            var tsunami = new EPSPTsunamiEventArgs();
            tsunami.IsExpired = true;
            tsunami.IsInvalidSignature = false;
            epspHandler.MediatorContext_OnTsunami(this, tsunami);
            Assert.AreEqual(string.Join("\r\n", messageList), GetWriterOutput());

            // キャンセル報
            tsunami.IsExpired = false;
            tsunami.IsCancelled = true;
            epspHandler.MediatorContext_OnTsunami(this, tsunami);
            messageList.Add("津波予報は解除されました。");
            Assert.AreEqual(string.Join("\r\n", messageList), GetWriterOutput());

            // 有効データ
            tsunami.IsCancelled = false;
            tsunami.RegionList = new List<TsunamiForecastRegion>();
            var region = new TsunamiForecastRegion();
            region.Category = TsunamiCategory.Advisory;
            region.IsImmediately = false;
            region.Region = "北海道太平洋沿岸東部";
            tsunami.RegionList.Add(region);
            var region2 = new TsunamiForecastRegion();
            region2.Category = TsunamiCategory.MajorWarning;
            region2.IsImmediately = false;
            region2.Region = "北海道太平洋沿岸中部";
            tsunami.RegionList.Add(region2);
            epspHandler.MediatorContext_OnTsunami(this, tsunami);
            messageList.Add("津波予報を受信しました: [津波注意報]北海道太平洋沿岸東部、[大津波警報]北海道太平洋沿岸中部");
            Assert.AreEqual(string.Join("\r\n", messageList), GetWriterOutput());
        }

        [TestCase]
        public void UserquakeTest()
        {
            var messageList = new List<string>();

            var userquake = new EPSPUserquakeEventArgs();
            userquake.IsInvalidSignature = false;
            userquake.IsExpired = false;
            userquake.AreaCode = "010";

            userquake.IsExpired = true;
            dateTime = DateTime.Parse("2018/01/01 12:00:00");
            epspHandler.MediatorContext_OnUserquake(this, userquake);
            Assert.AreEqual(string.Join("\r\n", messageList), GetWriterOutput());

            userquake.IsExpired = false;
            dateTime = DateTime.Parse("2018/01/01 12:00:00");
            epspHandler.MediatorContext_OnUserquake(this, userquake);
            Assert.AreEqual(string.Join("\r\n", messageList), GetWriterOutput());

            dateTime = DateTime.Parse("2018/01/01 12:00:29");
            epspHandler.MediatorContext_OnUserquake(this, userquake);
            Assert.AreEqual(string.Join("\r\n", messageList), GetWriterOutput());

            userquake.AreaCode = "011";
            dateTime = DateTime.Parse("2018/01/01 12:00:58");
            epspHandler.MediatorContext_OnUserquake(this, userquake);
            messageList.Add("地震感知情報の受信しきい値を超えました: 北海道 石狩(2)");
            Assert.AreEqual(string.Join("\r\n", messageList), GetWriterOutput());

            dateTime = DateTime.Parse("2018/01/01 12:01:27");
            userquake.AreaCode = "460";
            epspHandler.MediatorContext_OnUserquake(this, userquake);
            messageList.Add("地震感知情報が更新されました: 北海道 石狩(2)、大阪北部(1)");
            Assert.AreEqual(string.Join("\r\n", messageList), GetWriterOutput());

            dateTime = DateTime.Parse("2018/01/01 12:01:57");
            userquake.AreaCode = "460";
            epspHandler.MediatorContext_OnUserquake(this, userquake);
            Assert.AreEqual(string.Join("\r\n", messageList), GetWriterOutput());
        }


        private DateTime GetDateTime()
        {
            return dateTime;
        }

        private string GetWriterOutput()
        {
            return Regex.Replace(stringWriter.ToString().TrimEnd('\r', '\n'), @"^\d{4}/\d{2}/\d{2} \d{2}:\d{2}:\d{2}.\d{3} ", "", RegexOptions.Multiline);
        }
    }
}
