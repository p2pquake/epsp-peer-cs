using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using PKCSPeerCrypto;
using static PKCSPeerCrypto.Verifier;

namespace PKCSPeerCryptoTest
{
	[TestFixture]
    public class SignerTest
    {
        string privateKey = "MIIBBwIBADANBgkqhkiG9w0BAQEFAASB8jCB7wIBAAIxALDbI8NYvXosNN9SY0CzCq8c8Zv2UJpSt3M90Le+nKaaTyypzOKWFOQvNaoDNFbW2QIBEQIwE4GU4rjYqbXRICkWPblk8W58h8o51MkfWAlam11rgkp9xRGJpsBGP0Nxwh3zhvzBAhkA4mKz41EEqRnU7zv7efKRx09hsBlu9nppAhkAx/3FMacek2WfhvDxXqEGNJ53PA1pqYrxAhhqiM0frafXGzcHK0kqVAhdywDLV0NG0DECGF4dEYDHHXKKLPQ1JkqmIQmz++AGT9dQcQIYEEH0zolfobS0qHZk7GAyZh0kb6QSAx7/";
        string publicKey = "MEowDQYJKoZIhvcNAQEBBQADOQAwNgIxALDbI8NYvXosNN9SY0CzCq8c8Zv2UJpSt3M90Le+nKaaTyypzOKWFOQvNaoDNFbW2QIBEQ==";
        string keyExpire = "2017/04/04 23-37-41";
        string keySignature = "ttZhe/dyH7RioGsJx9IXHfwEdkeHe7UJIHqflpaPFAOQREvTZToU9/sq0D2LB8DDnywCyf+kaC4EfMduWl//joel8f+w0D7RiV2EX3eStIIyHTncbvi/HhdAu9PcZS2/5BZ1aIFY7YXbr8VjVq88TL72tDZxzkNlV1fgfsmFIBk=";

        [TestCase]
        public void SignUserquakeData()
        {
            DateTime signTime = DateTime.Parse("2017/04/04 23:36:41");
            DateTime verifyTime = DateTime.Parse("2017/04/04 23:37:41");

            // 署名
            var signResult = Signer.SignUserquake(1, privateKey, "901", signTime);

            // 検証
            Assert.AreEqual(signTime.AddMinutes(1), signResult.expire);
            var verifyResult = Verifier.VerifyUserquake(signResult.data, signResult.expire.ToString("yyyy/MM/dd HH-mm-ss"), signResult.signature, publicKey, keySignature, keyExpire, verifyTime);
            Assert.IsTrue(verifyResult.isValidSignature);
            Assert.IsFalse(verifyResult.isExpired);
        }

        [TestCase]
        public void SignUserquakeData_KeyExpired()
        {
            DateTime signTime = DateTime.Parse("2017/04/04 23:38:00");
            DateTime verifyTime = DateTime.Parse("2017/04/04 23:38:05");

            // 署名
            var signResult = Signer.SignUserquake(1, privateKey, "901", signTime);

            // 検証
            Assert.AreEqual(signTime.AddMinutes(1), signResult.expire);
            var verifyResult = Verifier.VerifyUserquake(signResult.data, signResult.expire.ToString("yyyy/MM/dd HH-mm-ss"), signResult.signature, publicKey, keySignature, keyExpire, verifyTime);
            Assert.IsTrue(verifyResult.isValidSignature);
            Assert.IsTrue(verifyResult.isExpired);
        }

        [TestCase]
        public void SignUserquakeData_DataExpired()
        {
            DateTime signTime = DateTime.Parse("2017/04/04 23:35:00");
            DateTime verifyTime = DateTime.Parse("2017/04/04 23:36:01");

            // 署名
            var signResult = Signer.SignUserquake(1, privateKey, "901", signTime);

            // 検証
            Assert.AreEqual(signTime.AddMinutes(1), signResult.expire);
            var verifyResult = Verifier.VerifyUserquake(signResult.data, signResult.expire.ToString("yyyy/MM/dd HH-mm-ss"), signResult.signature, publicKey, keySignature, keyExpire, verifyTime);
            Assert.IsTrue(verifyResult.isValidSignature);
            Assert.IsTrue(verifyResult.isExpired);
        }
    }
}
