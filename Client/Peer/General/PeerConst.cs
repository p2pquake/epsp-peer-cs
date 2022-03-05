using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Client.Peer.General
{
    class PeerConst
    {
        private static readonly Dictionary<int, string> codeMap = new()
        {
            { 550, "ReceiveReservedCode" },

            { 551, "NotifyEarthquake" },
            { 552, "NotifyTsunami" },
            { 553, "ReceiveReservedCode" },
            { 554, "ReceiveReservedCode" },
            { 555, "NotifyUserQuake" },
            { 556, "NotifyBbs" },
            { 557, "ReceiveReservedCode" },
            { 558, "ReceiveReservedCode" },
            { 559, "ReceiveReservedCode" },
            { 560, "ReceiveReservedCode" },
            { 561, "NotifyAreaPeer" },
            { 562, "ReceiveReservedCode" },
            { 563, "ReceiveReservedCode" },
            { 564, "ReceiveReservedCode" },
            { 565, "ReceiveReservedCode" },
            { 566, "ReceiveReservedCode" },
            { 567, "ReceiveReservedCode" },
            { 568, "ReceiveReservedCode" },
            { 569, "ReceiveReservedCode" },
            { 570, "ReceiveReservedCode" },
            { 571, "ReceiveReservedCode" },
            { 572, "ReceiveReservedCode" },
            { 573, "ReceiveReservedCode" },
            { 574, "ReceiveReservedCode" },
            { 575, "ReceiveReservedCode" },
            { 576, "ReceiveReservedCode" },
            { 577, "ReceiveReservedCode" },
            { 578, "ReceiveReservedCode" },
            { 579, "ReceiveReservedCode" },
            { 580, "ReceiveReservedCode" },
            { 581, "ReceiveReservedCode" },
            { 582, "ReceiveReservedCode" },
            { 583, "ReceiveReservedCode" },
            { 584, "ReceiveReservedCode" },
            { 585, "ReceiveReservedCode" },
            { 586, "ReceiveReservedCode" },
            { 587, "ReceiveReservedCode" },
            { 588, "ReceiveReservedCode" },
            { 589, "ReceiveReservedCode" },
            { 611, "RequirePeerEcho" },
            { 612, "RequirePeerId" },
            { 614, "RequireProtocolVersion" },
            { 615, "RequireInquiryEcho" },
            { 620, "ReceiveReservedCode" },
            { 621, "ReceiveReservedCode" },
            { 622, "ReceiveReservedCode" },
            { 623, "ReceiveReservedCode" },
            { 624, "ReceiveReservedCode" },
            { 625, "ReceiveReservedCode" },
            { 626, "ReceiveReservedCode" },
            { 627, "ReceiveReservedCode" },
            { 628, "ReceiveReservedCode" },
            { 629, "ReceiveReservedCode" },

            { 631, "ReplyPeerEcho" },
            { 632, "ReplyPeerId" },
            { 634, "ReplyProtocolVersion" },
            { 635, "ReplyInquiryEcho" },
            { 640, "ReceiveReservedCode" },
            { 641, "ReceiveReservedCode" },
            { 642, "ReceiveReservedCode" },
            { 643, "ReceiveReservedCode" },
            { 644, "ReceiveReservedCode" },
            { 645, "ReceiveReservedCode" },
            { 646, "ReceiveReservedCode" },
            { 647, "ReceiveReservedCode" },
            { 648, "ReceiveReservedCode" },
            { 649, "ReceiveReservedCode" },

            { 694, "InvalidProtocolVersion" },
            { 698, "InvalidOperation" },
        };

        public static string GetCodeName(int code)
        {
            if (codeMap.ContainsKey(code))
            {
                return codeMap[code];
            }

            return null;
        }
    }
}
