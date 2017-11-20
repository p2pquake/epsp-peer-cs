using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.Common.General;
using Client.Common.Net;
using Client.Peer.General;

namespace Client.Peer.Verifier
{
    class VerifyManager : IVerifyManager
    {
        public VerifyResult Verify(Packet packet)
        {
            if (packet.Code == Code.USERQUAKE)
            {
                return VerifyUserData(packet);
            }

            if (packet.Code == Code.AREAPEERS ||
                packet.Code == Code.EARTHQUAKE ||
                packet.Code == Code.TSUNAMI)
            {
                return VerifyServerData(packet);
            }

            // 知らないデータ形式
            Logger.GetLog().Warn("不明なデータ形式です。");

            VerifyResult result = new VerifyResult();
            result.IsValid = false;
            result.IsInvalidSignature = false;
            result.IsExpired = false;
            return result;
        }

        private VerifyResult VerifyUserData(Packet packet)
        {
            // FIXME
            throw new NotImplementedException();
        }

        private VerifyResult VerifyServerData(Packet packet)
        {
            // FIXME
            throw new NotImplementedException();
        }
    }
}
