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
        //private MemoryStream memoryStream;
        //private StreamWriter streamWriter;
        //private TextWriter textWriter;
        //private StreamReader streamReader;
        private StringWriter stringWriter;

        [SetUp]
        public void SetUp()
        {
            epspHandler = new EPSPHandler(GetDateTime);
            //memoryStream = new MemoryStream();
            //streamWriter = new StreamWriter(memoryStream);
            //textWriter = TextWriter.Synchronized(streamWriter);
            //streamWriter.AutoFlush = true;
            //Console.SetOut(textWriter);

            //streamReader = new StreamReader(memoryStream);

            stringWriter = new StringWriter();
            Console.SetOut(stringWriter);
        }

        [TearDown]
        public void TearDown()
        {
            //using (var std = new StreamWriter(Console.OpenStandardOutput()))
            //{
            //    std.AutoFlush = true;
            //    Console.SetOut(std);
            //}

            //streamReader.Close();
            //textWriter.Close();
            //streamWriter.Close();
            //memoryStream.Close();
        }

        [TestCase]
        public void UserquakeTest()
        {
            var messageList = new List<string>();

            var userquake = new EPSPUserquakeEventArgs();
            userquake.IsInvalidSignature = false;
            userquake.IsExpired = false;
            userquake.AreaCode = "010";

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
            return Regex.Replace(stringWriter.ToString().TrimEnd(), @"^\d{4}/\d{2}/\d{2} \d{2}:\d{2}:\d{2}.\d{3} ", "", RegexOptions.Multiline);
        }
    }
}
