using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.Client;
using Client.Client.General;

namespace Client.App
{
    public interface IPeerState
    {
        ClientState ClientState { get; }

        int PeerId { get; set; }

        TimeSpan TimeOffset { get; set; }

        int Connections { get; }

        bool IsPortOpened { get; }

        KeyData Key { get; set; }

        IDictionary<string, int> AreaPeerDictionary { get; set; }
    }
}
