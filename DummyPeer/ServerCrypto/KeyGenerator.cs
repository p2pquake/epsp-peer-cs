using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DummyPeer.ServerCrypto
{
    /// <summary>
    /// 鍵生成クラス
    /// </summary>
    public static class KeyGenerator
    {
        // TODO: HACK: 鍵の有効期限と鍵再割当ての許容時間とが別々に定義されていてわかりづらい
        private static readonly int PeerKeyBit = 384;
        private static readonly int PeerKeyExpireHours = 2;

        /// <summary>
        /// ユーザ鍵を生成します。
        /// </summary>
        /// <returns></returns>
        public static PeerKey GeneratePeerKey()
        {
            RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider(PeerKeyBit);

            PeerKey key = new PeerKey();
            key.RSAParam = rsaProvider.ExportParameters(true);
            key.Expire = DateTime.Now.AddHours(PeerKeyExpireHours); // TODO: HACK: PC時計に依存する
            key.KeySignature = SignPeerKey(key);

            return key;

        }

        /// <summary>
        /// ユーザ鍵の署名を生成します。
        /// 引数のユーザ鍵インスタンスは書き換えません。
        /// </summary>
        /// <param name="key"></param>
        /// <returns>署名</returns>
        private static string SignPeerKey(PeerKey key)
        {
            // 署名用データ生成
            byte[] expire = Encoding.ASCII.GetBytes(key.Expire.ToString("yyyy/MM/dd HH-mm-ss"));
            byte[] publicKey = key.PublicKeyBytes;
            byte[] signData = Enumerable.Concat(publicKey, expire).ToArray();

            SHA1CryptoServiceProvider sha1Provider = new SHA1CryptoServiceProvider();
            byte[] sha1SignData = sha1Provider.ComputeHash(signData);

            // 署名生成
            RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider();
            rsaProvider.ImportParameters(KeyDefine.PeerVerificationKey);

            RSAPKCS1SignatureFormatter pkcsFormatter = new RSAPKCS1SignatureFormatter(rsaProvider);
            pkcsFormatter.SetHashAlgorithm("SHA1");

            byte[] sign = pkcsFormatter.CreateSignature(sha1SignData);

            return Convert.ToBase64String(sign, Base64FormattingOptions.None);
        }
    }

}
