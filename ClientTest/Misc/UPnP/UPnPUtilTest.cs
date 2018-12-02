using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.Misc.UPnP;
using Client.Misc.UQSummary;
using NUnit.Framework;

namespace ClientTest.Misc.UQSummary
{
    [TestFixture]
    class UPnPUtilTest
    {
        [Test]
        public void OpenPort()
        {
            UPnPUtil.OpenPort(6910);
        }
    }
}
