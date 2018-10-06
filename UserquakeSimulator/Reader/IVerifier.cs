using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserquakeSimulator.Reader
{
    public interface IVerifier
    {
        bool HaveEarthquake(DateTime uqBegin, DateTime uqEnd, string[] prefecture);
    }
}
