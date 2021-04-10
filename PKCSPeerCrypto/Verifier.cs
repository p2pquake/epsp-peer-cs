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
        private static readonly string PeerProofKey =
            "MIGdMA0GCSqGSIb3DQEBAQUAA4GLADCBhwKBgQDTJKLLO7wjCHz80kpnisqcPDQvA9voNY5QuAA+bOWeqvl4gmPSiylzQZzldS+n/M5p4o1PRS24WAO+kPBHCf4ETAns8M02MFwxH/FlQnbvMfi9zutJkQAu3Hq4293rHz+iCQW/MWYB5IfzFBnWtEdjkhqHsGy6sZMMe+qx/F1rcQIBEQ==";

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
        /// 地震感知情報の署名を検証します．
        /// </summary>
        /// <param name="data">受信データ (ユニーク値,地域コード)</param>
        /// <param name="dataExpire">有効期限</param>
        /// <param name="dataSignature">署名</param>
        /// <param name="publicKey">公開鍵 (PKCS #8(DER)形式)</param>
        /// <param name="keySignature">鍵署名</param>
        /// <param name="keyExpire">鍵有効期限</param>
        /// <param name="now">プロトコル日時</param>
        /// <returns>検証結果</returns>
        public static VerifyResult VerifyUserquake(string data, string dataExpire, string dataSignature, string publicKey, string keySignature, string keyExpire, DateTime now)
        {
            // 鍵検証
            VerifyResult keyVerifyResult = VerifyKeySignature(publicKey, keySignature, keyExpire, now);
            if (keyVerifyResult.isExpired || !keyVerifyResult.isValidSignature)
            {
                return keyVerifyResult;
            }
            
            // TODO: VerifyServerDataとロジックほぼ重複
            // MD5導出
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            byte[] dataByteArray = Encoding.GetEncoding(932).GetBytes(data);
            MD5CryptoServiceProvider md5Provider = new MD5CryptoServiceProvider();
            byte[] dataMd5 = md5Provider.ComputeHash(dataByteArray);

            // EPSP日時
            byte[] expireByteArray = Encoding.ASCII.GetBytes(dataExpire);

            // 署名対象データ決定
            byte[] verifyData = Enumerable.Concat(expireByteArray, dataMd5).ToArray();

            // 署名検証
            VerifyResult verifyResult = new VerifyResult();
            verifyResult.isExpired = IsExpired(dataExpire, now);
            verifyResult.isValidSignature = IsValidSignature(verifyData, publicKey, dataSignature);
            return verifyResult;
        }
        
        private static VerifyResult VerifyKeySignature(string publicKey, string keySignature, string keyExpire, DateTime now)
        {
            VerifyResult verifyResult = new VerifyResult();
            verifyResult.isExpired = IsExpired(keyExpire, now);
            verifyResult.isValidSignature = false;

            try
            {
                byte[] verifyData = Enumerable.Concat(Convert.FromBase64String(publicKey), Encoding.ASCII.GetBytes(keyExpire)).ToArray();
                verifyResult.isValidSignature = IsValidSignature(verifyData, PeerProofKey, keySignature);
            } catch (FormatException e)
            {
                logger.Warn("署名検証時に例外が発生しました", e);
            }

            return verifyResult;
        }

        /// <summary>
        /// サーバ署名を検証します．
        /// </summary>
        /// <param name="data">データ</param>
        /// <param name="expire">有効期限</param>
        /// <param name="signature">署名</param>
        /// <param name="now">プロトコル日時</param>
        /// <returns></returns>
        public static VerifyResult VerifyServerData(string data, string expire, string signature, DateTime now)
        {
            // MD5導出
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            byte[] dataByteArray = Encoding.GetEncoding(932).GetBytes(data);
            MD5CryptoServiceProvider md5Provider = new MD5CryptoServiceProvider();
            byte[] dataMd5 = md5Provider.ComputeHash(dataByteArray);

            // EPSP日時
            byte[] expireByteArray = Encoding.ASCII.GetBytes(expire);

            // 署名対象データ決定
            byte[] verifyData = Enumerable.Concat(expireByteArray, dataMd5).ToArray();
            
            // 署名検証
            VerifyResult verifyResult = new VerifyResult();
            verifyResult.isExpired = IsExpired(expire, now);
            verifyResult.isValidSignature = IsValidSignature(verifyData, ServerProofKey, signature);
            return verifyResult;
        }


        private static bool IsValidSignature(byte[] data, string publicKey, string signature)
        {
            // 署名検証
            //   - 鍵設定
            RSAParameters rsaParams = Asn1PKCS.Decoder.PKCS8DERDecoder.DecodePublicKey(publicKey);
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.ImportParameters(rsaParams);
            
            //   - SHA1指定
            SHA1Managed sha1 = new SHA1Managed();
            byte[] sha1ByteArray = sha1.ComputeHash(data);

            RSAPKCS1SignatureDeformatter deformatter = new RSAPKCS1SignatureDeformatter(rsa);
            deformatter.SetHashAlgorithm("SHA1");
            
            //   - 検証
            try
            {
                byte[] signatureByteArray = Convert.FromBase64String(signature);
                return deformatter.VerifySignature(sha1ByteArray, signatureByteArray);
            }
            catch (FormatException e)
            {
                logger.Warn("署名検証時に例外が発生しました", e);
                return false;
            }
        }

        private static bool IsExpired(string expire, DateTime now)
        {
            try
            {
                DateTime expireDateTime = DateTime.ParseExact(expire, "yyyy/MM/dd HH-mm-ss", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None);
                return now > expireDateTime;
            }
            catch (FormatException e)
            {
                logger.Warn("日時検証時に例外が発生しました", e);
                return true;
            }
        }
    }
}
