using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PKCSPeerCrypto
{
    /// <summary>
    /// 地震感知情報の署名結果
    /// </summary>
    public struct UserquakeSignResult
    {
        /// <summary>
        /// 署名
        /// </summary>
        public string signature;
        /// <summary>
        /// 有効期限
        /// </summary>
        public DateTime expire;
        /// <summary>
        /// 送信データ (ユニーク値,地域コード)
        /// </summary>
        public string data;
    }

    /// <summary>
    /// ユーザ鍵による署名を行うクラス
    /// </summary>
    public class Signer
    {
        /// <summary>
        /// 地震感知情報のデータに対して署名を行います．
        /// </summary>
        /// <param name="peerId">ピアID (ユニーク値の生成に使用)</param>
        /// <param name="privateKey">秘密鍵 (PKCS #8(DER)形式)</param>
        /// <param name="areaCode">地域コード</param>
        /// <param name="now">プロトコル日時</param>
        /// <returns>署名結果</returns>
        public static UserquakeSignResult SignUserquake(int peerId, string privateKey, string areaCode, DateTime now)
        {
            DateTime expire = createExpire(now);
            string uniqueValue = createUniqueValue(peerId);
            string data = uniqueValue + "," + areaCode;

            string signature = Sign(privateKey, data, expire);

            UserquakeSignResult result = new UserquakeSignResult();
            result.data = data;
            result.expire = expire;
            result.signature = signature;

            return result;
        }
        
        private static DateTime createExpire(DateTime now)
        {
            return now.AddMinutes(1);
        }

        private static string createUniqueValue(int peerId)
        {
            return peerId.ToString("00000") + DateTime.Now.ToString("yyyyMMddHHmmssfff");
        }

        private static string Sign(string privateKey, string data, DateTime expire)
        {
            // MD5導出
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            byte[] dataByteArray = Encoding.GetEncoding(932).GetBytes(data);
            MD5CryptoServiceProvider md5Provider = new MD5CryptoServiceProvider();
            byte[] dataMd5 = md5Provider.ComputeHash(dataByteArray);

            // EPSP日時
            string expireStr = expire.ToString("yyyy/MM/dd HH-mm-ss");
            byte[] expireByteArray = Encoding.ASCII.GetBytes(expireStr);

            // 結合して署名
            byte[] signData = expireByteArray.Concat(dataMd5).ToArray();
            return Sign(privateKey, signData);
        }

        private static string Sign(string privateKey, byte[] data)
        {
            RSAParameters rsaParams = Asn1PKCS.Decoder.PKCS8DERDecoder.DecodePrivateKey(privateKey);
            
            SHA1CryptoServiceProvider sha1Provider = new SHA1CryptoServiceProvider();
            byte[] sha1SignData = sha1Provider.ComputeHash(data);
            
            RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider();
            rsaProvider.ImportParameters(rsaParams);

            RSAPKCS1SignatureFormatter pkcsFormatter = new RSAPKCS1SignatureFormatter(rsaProvider);
            pkcsFormatter.SetHashAlgorithm("SHA1");

            byte[] signature = pkcsFormatter.CreateSignature(sha1SignData);
            return Convert.ToBase64String(signature, Base64FormattingOptions.None);
        }
    }
}
