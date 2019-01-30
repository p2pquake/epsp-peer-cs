using Client.Peer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPSPWPFClient.Mediator
{
    class EPSPUQSummaryEventArgs : EPSPDataEventArgs
    {
        public IDictionary<string, int> Summary { get; set; }
    }
}
