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
        // Generate using BouncyCastle
        string privateKey = "MIIBCgIBADANBgkqhkiG9w0BAQEFAASB9TCB8gIBAAIxAKZgx9XkRZxXFHoIDTsYRn/2+2dmjQEp18tqXETO2KrIbBlhMgWNVn7BBaHmgdVytwIDAQABAjA7Wxaf556v7l8XRA0YUopAN8gr9OmECz1jUO++YJZqUN2Y0fsmp/DrWBThDg2eBxECGQD+VOV79uqO+/2oywYMO8ynprCZ50bvPGcCGQCneC6kROqLr7OPh0lZb5aEM0RIZWEwJTECGQDFNA68kDhPphSJTOLjmXqWhClrLGlUFwECGBz2Veqm2IFL45vg47iJ6df3Hcn/bm1rIQIYVNBOfkhwq273Ojf4L2B76jAyDJuQg5u2";
        string publicKey = "MEwwDQYJKoZIhvcNAQEBBQADOwAwOAIxAKZgx9XkRZxXFHoIDTsYRn/2+2dmjQEp18tqXETO2KrIbBlhMgWNVn7BBaHmgdVytwIDAQAB";
        string keyExpire = "2022/05/21 13-42-43";
        string keySignature = "wG4CZLdJS5NHz5CGCQU6eZ6USqoOlsOerX3h9x5AlzxKi0QMCOpIuQw+wVIAI587qTQmY3hph2OoAkOIFN4RKZL16tWRsIOw/04hh/8sfBlERvCOhgtv2AzUFVyTw0ATmoRLud2gYiLuMKor8p62phaI8sd/9LyqyKKS5bvITAs=";

        //string privateKey512 =
        //    "MIIBUwIBADANBgkqhkiG9w0BAQEFAASCAT0wggE5AgEAAkEAynwYvvEDzLA4Kd4Fn8pKpnHrNQ+Ipx4jTZWD9MgzlbEkGBU4ydQeGOOv8G+xtecXjimMcxFLyXJEQxwh47DsWQIDAQABAkBMopMrEUUC318iWWl/hKykOlFvD6YEuh9aadA7gNolOO0HmYckNV57+y5kqvhGMwOmiKD33AqGsgOFPahkmsBRAiEAz/FsCnLoBpes1XzimBQ0w95VIDo6OvpGsgZEI0l+ff8CIQD5R750gPwqJql5OtrJSBO8vN9eOMnUjFONX7ddE8lFpwIgIQU+Wd5rV/in+nLNbMbwphXdQLPeYVUy+rwh/9SA4LMCIF0MgbQdPYNCYTpGVMqTZAKsgbg6/GOhacK4uso8i4G7AiA2FV/VVJXmM2hRgVE1xBJCXv/NsHFibOowf4g0wT6E6Q==";
        //string publicKey512 =
        //    "MFwwDQYJKoZIhvcNAQEBBQADSwAwSAJBAMp8GL7xA8ywOCneBZ/KSqZx6zUPiKceI02Vg/TIM5WxJBgVOMnUHhjjr/BvsbXnF44pjHMRS8lyREMcIeOw7FkCAwEAAQ==";
        //string keyExpire512 =
        //    "2022/05/21 12-59-32";
        //string keySignature512 = "IA0VPy8sROXuzrCDTHKGHo3jEAhm7x+4nDXWGaMx8Dzyyo6AQa2mj7iAY1RsKtJhcxHUsqygXUr8JisFgIsxcqtFCxRQvwu1tf6PoDvspWOEm1XF+PSf2NvmSRzJCZMQylOuCTaFCg/EEykD0kHUpASrsX7SQixn1IIbsQhJQR0=";

        [TestCase]
        public void SignUserquakeData()
        {
            DateTime signTime = DateTime.Parse("2022/05/21 10:36:41");
            DateTime verifyTime = DateTime.Parse("2022/05/21 10:37:41");

            // 署名
            var signResult = Signer.SignUserquake(1, privateKey, "901", signTime);
            // Adaptation to client_vb6 implementation
            Assert.LessOrEqual(Convert.FromBase64String(signResult.signature).Length, 48);


            // 検証
            Assert.AreEqual(signTime.AddMinutes(1), signResult.expire);
            var verifyResult = Verifier.VerifyUserquake(signResult.data, signResult.expire.ToString("yyyy/MM/dd HH-mm-ss"), signResult.signature, publicKey, keySignature, keyExpire, verifyTime);
            Assert.IsTrue(verifyResult.isValidSignature);
            Assert.IsFalse(verifyResult.isExpired);
        }

        [TestCase]
        public void SignUserquakeData_KeyExpired()
        {
            DateTime signTime = DateTime.Parse("2022/05/21 13:42:40");
            DateTime verifyTime = DateTime.Parse("2022/05/21 13:42:44");

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
            DateTime signTime = DateTime.Parse("2022/05/21 10:35:00");
            DateTime verifyTime = DateTime.Parse("2022/05/21 10:36:01");

            // 署名
            var signResult = Signer.SignUserquake(1, privateKey, "901", signTime);

            // 検証
            Assert.AreEqual(signTime.AddMinutes(1), signResult.expire);
            var verifyResult = Verifier.VerifyUserquake(signResult.data, signResult.expire.ToString("yyyy/MM/dd HH-mm-ss"), signResult.signature, publicKey, keySignature, keyExpire, verifyTime);
            Assert.IsTrue(verifyResult.isValidSignature);
            Assert.IsTrue(verifyResult.isExpired);
        }

        // Note. EPSP Client (VB6) 版は、ピア署名鍵を 512 bits にできない。
        //       署名のバイト配列を 48 bytes (384 bits) しか確保していないため。
        //[TestCase]
        //public void SignUserquakeData_Key512Bits()
        //{
        //    DateTime signTime = DateTime.Parse("2017/04/04 23:36:41");
        //    DateTime verifyTime = DateTime.Parse("2017/04/04 23:37:41");

        //    // 署名
        //    var signResult = Signer.SignUserquake(1, privateKey512, "901", signTime);
        //    // Adaptation to client_vb6 implementation
        //    Assert.LessOrEqual(Convert.FromBase64String(signResult.signature).Length, 48);

        //    // 検証
        //    Assert.AreEqual(signTime.AddMinutes(1), signResult.expire);
        //    var verifyResult = Verifier.VerifyUserquake(signResult.data, signResult.expire.ToString("yyyy/MM/dd HH-mm-ss"), signResult.signature, publicKey512, keySignature512, keyExpire512, verifyTime);
        //    Assert.IsTrue(verifyResult.isValidSignature);
        //    Assert.IsFalse(verifyResult.isExpired);
        //}
    }
}
