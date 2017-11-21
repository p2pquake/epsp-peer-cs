using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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

        private byte[] GenerateSignatureData(string data, string expire)
        {
            // MD5導出
            byte[] dataByteArray = Encoding.GetEncoding(932).GetBytes(data);
            MD5CryptoServiceProvider md5Provider = new MD5CryptoServiceProvider();
            byte[] dataMd5 = md5Provider.ComputeHash(dataByteArray);

            // EPSP日時
            byte[] expireByteArray = Encoding.ASCII.GetBytes(expire);

            // 結合
            return dataMd5.Concat(expireByteArray).ToArray();
        }
    }
}
