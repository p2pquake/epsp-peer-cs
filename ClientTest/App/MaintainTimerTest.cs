using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Client.App;
using Client.App.State;
using Client.Client;
using Moq;
using NUnit.Framework;

namespace ClientTest.App
{
    [TestFixture]
    class MaintainTimerTest
    {
        [TestCase]
        public void LongTimeConnectingTest()
        {
            var mediatorContext = new MediatorContext();

            var maintainTimerField = mediatorContext.GetType().GetField("maintainTimer", BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance);
            var timer = (MaintainTimer)maintainTimerField.GetValue(mediatorContext);
            var intervalField = timer.GetType().GetField("maintainInterval", BindingFlags.SetField | BindingFlags.NonPublic | BindingFlags.Instance);
            intervalField.SetValue(timer, 1000);

            var field = mediatorContext.GetType().GetField("clientContext", BindingFlags.SetField | BindingFlags.NonPublic | BindingFlags.Instance);
            var clientContextMock = new Mock<IClientContext>();
            field.SetValue(mediatorContext, clientContextMock.Object);

            bool isCalled = false;
            mediatorContext.StateChanged += (s, e) =>
            {
                Console.WriteLine("State: " + mediatorContext.ReadonlyState);
                if (mediatorContext.ReadonlyState is DisconnectedState)
                {
                    isCalled = true;
                }
            };
            mediatorContext.Completed += (s, e) => { };
            mediatorContext.ConnectionsChanged += (s, e) => { };

            mediatorContext.Connect();

            int waitSeconds = 120;
            while (waitSeconds-- > 0 && !isCalled)
            {
                Thread.Sleep(1000);
            }

            Assert.IsTrue(isCalled);
        }
    }
}
