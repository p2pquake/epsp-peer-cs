using Client.Peer;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPSPWPFClient.Userquake
{
    public class EPSPUQSummaryEventArgs : EPSPDataEventArgs
    {
        private IDictionary<string, int> _summary;

        public DateTime StartedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public IDictionary<string, int> Summary
        {
            get => _summary;
            set { _summary = value; CallPropertyChanged("Title"); } // HACK: とりあえず動くが雑
        }
    }
}
