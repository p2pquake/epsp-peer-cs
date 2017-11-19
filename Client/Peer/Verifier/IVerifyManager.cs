using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.Common.Net;

namespace Client.Peer.Verifier
{
    struct VerifyResult
    {
        bool IsValid;
        bool IsInvalidSignature;
        bool IsExpired;
    }

    interface IVerifyManager
    {
        VerifyResult Verify(Packet packet);
    }
}
