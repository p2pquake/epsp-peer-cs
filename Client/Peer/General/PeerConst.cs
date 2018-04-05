using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Client.Peer.General
{
    class PeerConst
    {
        public static string getCodeName(int code)
        {
            Dictionary<int, string> codeMap = new Dictionary<int, string>()
            {
                { 551, "NotifyEarthquake" },
                { 552, "NotifyTsunami" },
                { 555, "NotifyUserQuake" },
                { 556, "NotifyBbs" },
                { 561, "NotifyAreaPeer" },
                { 611, "RequirePeerEcho" },
                { 612, "RequirePeerId" },
                { 614, "RequireProtocolVersion" },
                { 615, "RequireInquiryEcho" },
                
                { 631, "ReplyPeerEcho" },
                { 632, "ReplyPeerId" },
                { 634, "ReplyProtocolVersion" },
                { 635, "ReplyInquiryEcho" },

                { 694, "InvalidProtocolVersion" },
                { 698, "InvalidOperation" },
            };

            if (codeMap.ContainsKey(code))
            {
                return codeMap[code];
            }

            return null;
        }
    }
}
