using Client.Peer;
using EPSPWPFClient.Userquake;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPSPWPFClient.Mediator
{
    interface IEPSPHandleable
    {
        void OnEarthquake(EPSPQuakeEventArgs e);
        void OnTsunami(EPSPTsunamiEventArgs e);
        void OnUserquake(EPSPUserquakeEventArgs e);
        void OnEEWTest(EPSPEEWTestEventArgs e);
        void OnAreapeers(EPSPAreapeersEventArgs e);
        void OnUserquakeReached(EPSPUQSummaryEventArgs e);
        void OnUserquakeUpdated(EPSPUQSummaryEventArgs e);
    }
}
