using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.Common.Net.UPnP;
using NUnit.Framework;

namespace ClientTest.Common.Net.UPnP
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
