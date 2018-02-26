using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace PKCSPeerCrypto
{
    /// <summary>
    /// 署名検証を行うクラス
    /// </summary>
    public class Verifier
    {
        private static ILog logger = LogManager.GetLogger("Verifier");

        private static readonly string ServerProofKey =
            "MIGdMA0GCSqGSIb3DQEBAQUAA4GLADCBhwKBgQC8p/vth2yb/k9x2/PcXKdb6oI3gAbhvr/HPTOwla5tQHB83LXNF4Y+Sv/Mu4Uu0tKWz02FrLgA5cuJZfba9QNULTZLTNUgUXIB0m/dq5Rx17IyCfLQ2XngmfFkfnRdRSK7kGnIXvO2/LOKD50JsTf2vz0RQIdw6cEmdl+Aga7i8QIBEQ==";

        /// <summary>
        /// 検証結果
        /// </summary>
        public struct VerifyResult
        {
            /// <summary>
            /// 期限切れ
            /// </summary>
            public bool isExpired;
            /// <summary>
            /// 正しい署名
            /// </summary>
            public bool isValidSignature;
        }

        /// <summary>
        /// サーバ署名を検証します．
        /// </summary>
        /// <param name="data">データ</param>
        /// <param name="expire">有効期限</param>
        /// <param name="signature">署名</param>
        /// <returns></returns>
        public static VerifyResult VerifyServerData(string data, string expire, string signature, DateTime now)
        {
            // MD5導出
            byte[] dataByteArray = Encoding.GetEncoding(932).GetBytes(data);
            MD5CryptoServiceProvider md5Provider = new MD5CryptoServiceProvider();
            byte[] dataMd5 = md5Provider.ComputeHash(dataByteArray);

            // EPSP日時
            byte[] expireByteArray = Encoding.ASCII.GetBytes(expire);

            // 署名対象データ決定
            byte[] verifyData = Enumerable.Concat(expireByteArray, dataMd5).ToArray();

            // 署名検証
            //   - 鍵設定
            RSAParameters rsaParams = Asn1PKCS.Decoder.PKCS8DERDecoder.DecodePublicKey(ServerProofKey);
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.ImportParameters(rsaParams);
            
            //   - SHA1指定
            SHA1Managed sha1 = new SHA1Managed();
            byte[] sha1ByteArray = sha1.ComputeHash(verifyData);

            RSAPKCS1SignatureDeformatter deformatter = new RSAPKCS1SignatureDeformatter(rsa);
            deformatter.SetHashAlgorithm("SHA1");

            // 署名検証
            VerifyResult verifyResult = new VerifyResult();
            verifyResult.isExpired = true;
            verifyResult.isValidSignature = false;

            try
            {
                byte[] signatureByteArray = Convert.FromBase64String(signature);
                bool isValidSignature = deformatter.VerifySignature(sha1ByteArray, signatureByteArray);

                // 結果設定
                verifyResult.isValidSignature = isValidSignature;
                DateTime expireDateTime = DateTime.ParseExact(expire, "yyyy/MM/dd HH-mm-ss", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None);
                verifyResult.isExpired = now > expireDateTime;
            } catch (FormatException e)
            {
                logger.Warn("署名検証時に例外が発生しました", e);
            }

            return verifyResult;
        }
    }
}
