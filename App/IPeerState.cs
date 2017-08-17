using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.Client.General;

namespace Client.App
{
    interface IPeerState
    {
        int PeerId { get; set; }

        TimeSpan TimeOffset { get; set; }

        KeyData Key { get; set; }
    }
}
