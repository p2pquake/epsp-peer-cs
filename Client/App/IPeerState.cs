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
        int PeerId { get; }

        TimeSpan TimeOffset { get; }

        int Connections { get; }

        bool IsPortOpened { get; }

        KeyData Key { get; }

        IDictionary<string, int> AreaPeerDictionary { get; }

        DateTime CalcNowProtocolTime();
    }

    interface IPeerStateForClient
    {
        int PeerId { get; set; }

        TimeSpan TimeOffset { get; set; }

        int Connections { get; }

        bool IsPortOpened { set; }

        KeyData Key { get; set; }

        IDictionary<string, int> AreaPeerDictionary { get; set; }

        DateTime CalcNowProtocolTime();
    }

    interface IPeerStateForPeer
    {
        int PeerId { get; }

        TimeSpan TimeOffset { get; }

        KeyData Key { get; }

        IDictionary<string, int> AreaPeerDictionary { get; set; }

        DateTime CalcNowProtocolTime();
    }
}
