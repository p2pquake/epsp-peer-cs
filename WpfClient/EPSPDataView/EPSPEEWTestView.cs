using Client.Peer;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfClient.EPSPDataView
{
    public class EPSPEEWTestView
    {
        public EPSPEEWTestEventArgs EventArgs { get; init; }

        public string Time => EventArgs.ReceivedAt.ToString("dd日HH時mm分");
    }
}
