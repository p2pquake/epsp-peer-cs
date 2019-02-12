using Client.Peer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPSPWPFClient.Userquake
{
    public class EPSPUQSummaryEventArgs : EPSPDataEventArgs
    {
        public DateTime StartedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public IDictionary<string, int> Summary { get; set; }
    }
}
