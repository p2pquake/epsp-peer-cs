using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace ClientTest
{
    [TestFixture]
    public class SampleTest
    {
        [TestCase]
        public void SampleTest_001()
        {
            Assert.Pass("NUnit works!");
        }
    }
}
