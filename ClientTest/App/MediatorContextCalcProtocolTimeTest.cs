using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Client.App;
using Client.Client;
using Client.Client.State;
using Client.Common.Net;
using NUnit.Framework;

namespace ClientTest.App
{
    [TestFixture]
    class MediatorContextCalcProtocolTimeTest
    {
        [TestCase]
        public void OffsetDefaultTest()
        {
            var mediatorContext = new MediatorContext();

            var now = DateTime.Now;
            var protocolTime = mediatorContext.CalcNowProtocolTime();

            Assert.AreEqual(0, (protocolTime - now).TotalSeconds, 1.0);
        }

        [TestCase]
        public void OffsetZeroTest()
        {
            var mediatorContext = new MediatorContext();
            SetOffset(mediatorContext, DateTime.Now);

            var now = DateTime.Now;
            var protocolTime = mediatorContext.CalcNowProtocolTime();

            Assert.AreEqual(0, (protocolTime - now).TotalSeconds, 1.0);
        }

        [TestCase]
        public void OffsetPlusTest()
        {
            var mediatorContext = new MediatorContext();
            SetOffset(mediatorContext, DateTime.Now.AddSeconds(5));

            var now = DateTime.Now;
            var protocolTime = mediatorContext.CalcNowProtocolTime();

            Assert.AreEqual(5, (protocolTime - now).TotalSeconds, 1.0);
        }

        [TestCase]
        public void OffsetMinusTest()
        {
            var mediatorContext = new MediatorContext();
            SetOffset(mediatorContext, DateTime.Now.AddSeconds(-5));

            var now = DateTime.Now;
            var protocolTime = mediatorContext.CalcNowProtocolTime();

            Assert.AreEqual(-5, (protocolTime - now).TotalSeconds, 1.0);
        }

        private void SetOffset(MediatorContext mediatorContext, DateTime protocolTime) 
        {
            var field = mediatorContext.GetType().GetField("clientContext", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance);
            var clientContext = (ClientContext)field.GetValue(mediatorContext);

            var requireProtocolTimeState = new RequireProtocolTimeState();
            var packet = new Packet();
            packet.Data = new string[] { protocolTime.ToString("yyyy/MM/dd HH-mm-ss") };
            requireProtocolTimeState.ReceiveProtocolTime(clientContext, null, packet);
        }
    }
}
