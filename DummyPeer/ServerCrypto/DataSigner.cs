using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DummyPeer.ServerCrypto
{
    public static class DataSigner
    {
        static DataSigner()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public static string SignInformation(string data)
        {
            // 署名用データ生成
            byte[] signData = Encoding.GetEncoding(932).GetBytes(data);
            return SignInformation(signData);
        }

        public static string SignInformation(byte[] data)
        {
            SHA1CryptoServiceProvider sha1Provider = new SHA1CryptoServiceProvider();
            byte[] sha1SignData = sha1Provider.ComputeHash(data);

            // 署名生成
            RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider();
            rsaProvider.ImportParameters(KeyDefine.ServerVerificationKey);

            RSAPKCS1SignatureFormatter pkcsFormatter = new RSAPKCS1SignatureFormatter(rsaProvider);
            pkcsFormatter.SetHashAlgorithm("SHA1");

            byte[] sign = pkcsFormatter.CreateSignature(sha1SignData);
            return Convert.ToBase64String(sign, Base64FormattingOptions.None);
        }

        public static string SignServerData(string data, DateTime expire)
        {
            // "EPSP日時 + Shift-JISバイト列のMD5"のバイト列を署名する

            // MD5導出
            byte[] dataByteArray = Encoding.GetEncoding(932).GetBytes(data);
            MD5CryptoServiceProvider md5Provider = new MD5CryptoServiceProvider();
            byte[] dataMd5 = md5Provider.ComputeHash(dataByteArray);

            // EPSP日時
            string expireStr = expire.ToString("yyyy/MM/dd HH-mm-ss");
            byte[] expireByteArray = Encoding.ASCII.GetBytes(expireStr);

            // 結合して署名
            byte[] signData = expireByteArray.Concat(dataMd5).ToArray();
            return SignInformation(signData);
        }
    }
}
