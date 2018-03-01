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
    public class VerifierTest
    {
		[TestCase]
		public void VerifyServerData_JMAQuake_Correct()
        {
            string signature = "b82CtnhEV8eQZGo3qdWuw9zbBtPIdWj1OEPYnPBda/x3V8LXMEs4LtebiRkM+ennoABX+v6Ko3fcFuyv3R45SmDnABcco8ziAxHbnvaFKt70UKxGExE6N4jtWjcS2qiOgGtzpGL5PCBolChmxmLaGqVqBsoBGJr90BakKyUko4o=";
			string expire = "2018/02/26 01-37-08";
            string summary = "26日1時28分,4,2,1,,,,0,,,気象庁";
			string detail = "-福島県,+4,*福島県中通り,*福島県浜通り,-宮城県,+3,*宮城県北部,*宮城県南部,*宮城県中部,-茨城県,+3,*茨城県北部,*茨城県南部,-栃木県,+3,*栃木県北部,*栃木県南部";

            VerifyResult result = Verifier.VerifyServerData(summary + detail, expire, signature, DateTime.Parse("2018/02/26 01:37:08"));
            Assert.IsFalse(result.isExpired);
            Assert.IsTrue(result.isValidSignature);
        }

        [TestCase]
        public void VerifyServerData_JMAQuake_Expired()
        {
            string signature = "aOOBMtic8+BxJyZqg7ctWmuS77a89rapTY4qqs4/AHbdUDFdT05w2lxEW+arYnAiaLhvSuZF9UcFeDgm3Et7LU2qMRKASLqOJLEDpX5R1TQHq8IEp4M6NfS3UyI6GDcEJw5yxcmfqFjy2nfVqcb4m1LeP8HCsuCJa9ZaLA5IBU0=";
            string expire = "2018/02/26 01-38-07";
            string summary = "26日1時28分,3以上,0,2,福島県沖,40km,5.7,0,N37.5,E141.8,";
            string detail = "";

            VerifyResult result = Verifier.VerifyServerData(summary + detail, expire, signature, DateTime.Parse("2018/02/26 01:38:08"));
            Assert.IsTrue(result.isExpired);
            Assert.IsTrue(result.isValidSignature);
        }

        [TestCase]
        public void VerifyServerData_JMAQuake_InvalidSignature_Edited()
        {
            string signature = "HI9hWa/uVPFhfAjQ//rqcFgJJOq3ISkxyMl2x6vG/SJJhu9JDWR+sf0i9a6kByyRh4YvRSkfvPXQo539VCGQlWWYeH4bq4pihIzhG0EXb20Bci9pmpbM2yxTk4vIp0vAnLTh5qyLe4CKVjwm4yvSDaJ93wZsrMzj3l4nKqLSphM=";
            string expire = "2018/02/26 01-39-08";
            string summary = "26日1時28分,4,0,3,福島県沖,40km,5.7,0,N37.5,E141.8,気象庁";
            string detail = "-福島県,+4,*福島県中通り,*福島県浜通り,-宮城県,+3,*宮城県北部,*宮城県南部,*宮城県中部,-福島県,+3,*福島県会津,-茨城県,+3,*茨城県北部,*茨城県南部,-栃木県,+3,*栃木県北部,*栃木県南部,-群馬県,+3,*群馬県南部,-埼玉県,+3,*埼玉県南部,-千葉県,+3,*千葉県北西部";

            VerifyResult result = Verifier.VerifyServerData(summary + detail, expire, signature, DateTime.Parse("2018/02/26 01:39:08"));
            Assert.IsFalse(result.isExpired);
            Assert.IsFalse(result.isValidSignature);
        }

        [TestCase]
        public void VerifyServerData_JMAQuake_InvalidSignature_Another()
        {
            string signature = "aOOBMtic8+BxJyZqg7ctWmuS77a89rapTY4qqs4/AHbdUDFdT05w2lxEW+arYnAiaLhvSuZF9UcFeDgm3Et7LU2qMRKASLqOJLEDpX5R1TQHq8IEp4M6NfS3UyI6GDcEJw5yxcmfqFjy2nfVqcb4m1LeP8HCsuCJa9ZaLA5IBU0=";
            string expire = "2018/02/26 01-39-08";
            string summary = "26日1時28分,4,0,3,福島県沖,40km,5.7,0,N37.5,E141.8,気象庁";
            string detail = "-福島県,+4,*福島県中通り,*福島県浜通り,-宮城県,+3,*宮城県北部,*宮城県南部,*宮城県中部,-福島県,+3,*福島県会津,-茨城県,+3,*茨城県北部,*茨城県南部,-栃木県,+3,*栃木県北部,*栃木県南部,-群馬県,+3,*群馬県南部,-埼玉県,+3,*埼玉県南部,-千葉県,+3,*千葉県北西部";

            VerifyResult result = Verifier.VerifyServerData(summary + detail, expire, signature, DateTime.Parse("2018/02/26 01:39:08"));
            Assert.IsFalse(result.isExpired);
            Assert.IsFalse(result.isValidSignature);
        }

		[TestCase]
		public void VerifyServerData_CorruptedSignature()
        {
            string signature = "SIGNATURE_CORRUPTED";
            string expire = "2018/02/26 01-39-08";
            string summary = "26日1時28分,4,0,3,福島県沖,40km,5.7,0,N37.5,E141.8,気象庁";
            string detail = "-福島県,+4,*福島県中通り,*福島県浜通り,-宮城県,+3,*宮城県北部,*宮城県南部,*宮城県中部,-福島県,+3,*福島県会津,-茨城県,+3,*茨城県北部,*茨城県南部,-栃木県,+3,*栃木県北部,*栃木県南部,-群馬県,+3,*群馬県南部,-埼玉県,+3,*埼玉県南部,-千葉県,+3,*千葉県北西部";

            VerifyResult result = Verifier.VerifyServerData(summary + detail, expire, signature, DateTime.Parse("2018/02/26 01:39:08"));
            Assert.IsFalse(result.isValidSignature);
        }

        [TestCase]
        public void VerifyServerData_InvalidSignature()
        {
            string signature = "VEVTVA==";
            string expire = "2018/02/26 01-39-08";
            string summary = "26日1時28分,4,0,3,福島県沖,40km,5.7,0,N37.5,E141.8,気象庁";
            string detail = "-福島県,+4,*福島県中通り,*福島県浜通り,-宮城県,+3,*宮城県北部,*宮城県南部,*宮城県中部,-福島県,+3,*福島県会津,-茨城県,+3,*茨城県北部,*茨城県南部,-栃木県,+3,*栃木県北部,*栃木県南部,-群馬県,+3,*群馬県南部,-埼玉県,+3,*埼玉県南部,-千葉県,+3,*千葉県北西部";

            VerifyResult result = Verifier.VerifyServerData(summary + detail, expire, signature, DateTime.Parse("2018/02/26 01:39:08"));
            Assert.IsFalse(result.isValidSignature);
        }

        [TestCase]
        public void VerifyServerData_CorruptedExpire()
        {
            string signature = "aOOBMtic8+BxJyZqg7ctWmuS77a89rapTY4qqs4/AHbdUDFdT05w2lxEW+arYnAiaLhvSuZF9UcFeDgm3Et7LU2qMRKASLqOJLEDpX5R1TQHq8IEp4M6NfS3UyI6GDcEJw5yxcmfqFjy2nfVqcb4m1LeP8HCsuCJa9ZaLA5IBU0=";
            string expire = "DATE_CORRUPTED";
            string summary = "26日1時28分,4,0,3,福島県沖,40km,5.7,0,N37.5,E141.8,気象庁";
            string detail = "-福島県,+4,*福島県中通り,*福島県浜通り,-宮城県,+3,*宮城県北部,*宮城県南部,*宮城県中部,-福島県,+3,*福島県会津,-茨城県,+3,*茨城県北部,*茨城県南部,-栃木県,+3,*栃木県北部,*栃木県南部,-群馬県,+3,*群馬県南部,-埼玉県,+3,*埼玉県南部,-千葉県,+3,*千葉県北西部";

            VerifyResult result = Verifier.VerifyServerData(summary + detail, expire, signature, DateTime.Parse("2018/02/26 01:39:08"));
            Assert.IsTrue(result.isExpired);
        }

        [TestCase]
        public void VerifyUserquake_Correct()
        {
            string dataSignature = "0C/bVWbyqNRCbOijje6ihB/Xk+gcaBiBj8oN13kpP8n9CbesxIidbLp1EzcVAa/C";
            string dataExpire = "2018/02/27 03-29-32";
            string publicKey = "MEowDQYJKoZIhvcNAQEBBQADOQAwNgIxAOhqFvL7N/G2RlrluixV6efWsAaAA+9albM86Ltv9MJjwLBiJYAXZPI9UJY++COYOwIBEQ==";
            string keySignature = "SMCL2D6mR+3LWCa3A5a+j5m3+6G1Utt0zcvVMQTiBhI657RG0n008rNAEuAF02YhA12cNhT6Rji9op7dmPNQhRGDvLCeYQPJuA482Q42Ld4iH9CPNUDDxOJMKN/hJC8LSb0cL+Q7RaCml/+J07cI9VG4nHC9f0X2dhQ5790RkKM=";
            string keyExpire = "2018/02/27 04-17-21";
            string data = "1638_270328,525";

            VerifyResult result = Verifier.VerifyUserquake(data, dataExpire, dataSignature, publicKey, keySignature, keyExpire, DateTime.Parse("2018/02/27 03:29:32"));
            Assert.IsFalse(result.isExpired);
            Assert.IsTrue(result.isValidSignature);
        }

        [TestCase]
        public void VerifyUserquake_DataIsExpired()
        {
            string dataSignature = "0C/bVWbyqNRCbOijje6ihB/Xk+gcaBiBj8oN13kpP8n9CbesxIidbLp1EzcVAa/C";
            string dataExpire = "2018/02/27 03-29-32";
            string publicKey = "MEowDQYJKoZIhvcNAQEBBQADOQAwNgIxAOhqFvL7N/G2RlrluixV6efWsAaAA+9albM86Ltv9MJjwLBiJYAXZPI9UJY++COYOwIBEQ==";
            string keySignature = "SMCL2D6mR+3LWCa3A5a+j5m3+6G1Utt0zcvVMQTiBhI657RG0n008rNAEuAF02YhA12cNhT6Rji9op7dmPNQhRGDvLCeYQPJuA482Q42Ld4iH9CPNUDDxOJMKN/hJC8LSb0cL+Q7RaCml/+J07cI9VG4nHC9f0X2dhQ5790RkKM=";
            string keyExpire = "2018/02/27 04-17-21";
            string data = "1638_270328,525";

            VerifyResult result = Verifier.VerifyUserquake(data, dataExpire, dataSignature, publicKey, keySignature, keyExpire, DateTime.Parse("2018/02/27 03:29:33"));
            Assert.IsTrue(result.isExpired);
            Assert.IsTrue(result.isValidSignature);
        }

        [TestCase]
        public void VerifyUserquake_InvalidDataSignature_Edited()
        {
            string dataSignature = "1C/bVWbyqNRCbOijje6ihB/Xk+gcaBiBj8oN13kpP8n9CbesxIidbLp1EzcVAa/C";
            string dataExpire = "2018/02/27 03-29-32";
            string publicKey = "MEowDQYJKoZIhvcNAQEBBQADOQAwNgIxAOhqFvL7N/G2RlrluixV6efWsAaAA+9albM86Ltv9MJjwLBiJYAXZPI9UJY++COYOwIBEQ==";
            string keySignature = "SMCL2D6mR+3LWCa3A5a+j5m3+6G1Utt0zcvVMQTiBhI657RG0n008rNAEuAF02YhA12cNhT6Rji9op7dmPNQhRGDvLCeYQPJuA482Q42Ld4iH9CPNUDDxOJMKN/hJC8LSb0cL+Q7RaCml/+J07cI9VG4nHC9f0X2dhQ5790RkKM=";
            string keyExpire = "2018/02/27 04-17-21";
            string data = "1638_270328,525";

            VerifyResult result = Verifier.VerifyUserquake(data, dataExpire, dataSignature, publicKey, keySignature, keyExpire, DateTime.Parse("2018/02/27 03:29:32"));
            Assert.IsFalse(result.isExpired);
            Assert.IsFalse(result.isValidSignature);
        }

        [TestCase]
        public void VerifyUserquake_InvalidDataSignature_Another()
        {
            string dataSignature = "bgU4UOzGjfRKQZLq4Bzr4cShdBsCDnK/UBd5RbCqJ0I0dU97mFLLaNLAKscgN8iX";
            string dataExpire = "2018/02/27 03-29-32";
            string publicKey = "MEowDQYJKoZIhvcNAQEBBQADOQAwNgIxAOhqFvL7N/G2RlrluixV6efWsAaAA+9albM86Ltv9MJjwLBiJYAXZPI9UJY++COYOwIBEQ==";
            string keySignature = "SMCL2D6mR+3LWCa3A5a+j5m3+6G1Utt0zcvVMQTiBhI657RG0n008rNAEuAF02YhA12cNhT6Rji9op7dmPNQhRGDvLCeYQPJuA482Q42Ld4iH9CPNUDDxOJMKN/hJC8LSb0cL+Q7RaCml/+J07cI9VG4nHC9f0X2dhQ5790RkKM=";
            string keyExpire = "2018/02/27 04-17-21";
            string data = "1638_270328,525";

            VerifyResult result = Verifier.VerifyUserquake(data, dataExpire, dataSignature, publicKey, keySignature, keyExpire, DateTime.Parse("2018/02/27 03:29:32"));
            Assert.IsFalse(result.isExpired);
            Assert.IsFalse(result.isValidSignature);
        }

        [TestCase]
        public void VerifyUserquake_KeyIsExpired()
        {
            string dataSignature = "0C/bVWbyqNRCbOijje6ihB/Xk+gcaBiBj8oN13kpP8n9CbesxIidbLp1EzcVAa/C";
            string dataExpire = "2018/02/27 03-29-32";
            string publicKey = "MEowDQYJKoZIhvcNAQEBBQADOQAwNgIxAOhqFvL7N/G2RlrluixV6efWsAaAA+9albM86Ltv9MJjwLBiJYAXZPI9UJY++COYOwIBEQ==";
            string keySignature = "SMCL2D6mR+3LWCa3A5a+j5m3+6G1Utt0zcvVMQTiBhI657RG0n008rNAEuAF02YhA12cNhT6Rji9op7dmPNQhRGDvLCeYQPJuA482Q42Ld4iH9CPNUDDxOJMKN/hJC8LSb0cL+Q7RaCml/+J07cI9VG4nHC9f0X2dhQ5790RkKM=";
            string keyExpire = "2018/02/27 04-17-21";
            string data = "1638_270328,525";

            VerifyResult result = Verifier.VerifyUserquake(data, dataExpire, dataSignature, publicKey, keySignature, keyExpire, DateTime.Parse("2018/02/27 04:17:22"));
            Assert.IsTrue(result.isExpired);
            Assert.IsTrue(result.isValidSignature);
        }

        [TestCase]
        public void VerifyUserquake_InvalidKeySignature_Edited()
        {
            string dataSignature = "0C/bVWbyqNRCbOijje6ihB/Xk+gcaBiBj8oN13kpP8n9CbesxIidbLp1EzcVAa/C";
            string dataExpire = "2018/02/27 03-29-32";
            string publicKey = "MEowDQYJKoZIhvcNAQEBBQADOQAwNgIxAOhqFvL7N/G2RlrluixV6efWsAaAA+9albM86Ltv9MJjwLBiJYAXZPI9UJY++COYOwIBEQ==";
            string keySignature = "SMCL2D6MR+3LWCa3A5a+j5m3+6G1Utt0zcvVMQTiBhI657RG0n008rNAEuAF02YhA12cNhT6Rji9op7dmPNQhRGDvLCeYQPJuA482Q42Ld4iH9CPNUDDxOJMKN/hJC8LSb0cL+Q7RaCml/+J07cI9VG4nHC9f0X2dhQ5790RkKM=";
            string keyExpire = "2018/02/27 04-17-21";
            string data = "1638_270328,525";

            VerifyResult result = Verifier.VerifyUserquake(data, dataExpire, dataSignature, publicKey, keySignature, keyExpire, DateTime.Parse("2018/02/27 03:29:32"));
            Assert.IsFalse(result.isExpired);
            Assert.IsFalse(result.isValidSignature);
        }

        [TestCase]
        public void VerifyUserquake_InvalidKeySignature_Another()
        {
            string dataSignature = "0C/bVWbyqNRCbOijje6ihB/Xk+gcaBiBj8oN13kpP8n9CbesxIidbLp1EzcVAa/C";
            string dataExpire = "2018/02/27 03-29-32";
            string publicKey = "MEowDQYJKoZIhvcNAQEBBQADOQAwNgIxAOhqFvL7N/G2RlrluixV6efWsAaAA+9albM86Ltv9MJjwLBiJYAXZPI9UJY++COYOwIBEQ==";
            string keySignature = "c834gslXCfQLEv7iNnvkwJgyliQBHEJWGQhRmOOLuGF/YPo47ar4mB4TILm1MnIPNBRGHo2SsNh1GzdxPoILStB4Tbg9r7x3s40YDdZzo/3dW6HAnEtwQeponMSy1zsYr1AP3LKIwRaRxvCuwltOhfCfkde0Bqf2BoCHrC++wvc=";
            string keyExpire = "2018/02/27 09-32-17";
            string data = "1638_270328,525";

            VerifyResult result = Verifier.VerifyUserquake(data, dataExpire, dataSignature, publicKey, keySignature, keyExpire, DateTime.Parse("2018/02/27 03:29:32"));
            Assert.IsFalse(result.isExpired);
            Assert.IsFalse(result.isValidSignature);
        }

        [TestCase]
        public void VerifyUserquake_AnotherPublicKey()
        {
            string dataSignature = "0C/bVWbyqNRCbOijje6ihB/Xk+gcaBiBj8oN13kpP8n9CbesxIidbLp1EzcVAa/C";
            string dataExpire = "2018/02/27 03-29-32";
            string publicKey = "MEwwDQYJKoZIhvcNAQEBBQADOwAwOAIxAIMdh5rapUc4iT43bdU5ZsegZ0FR2K7/ESoB8xLl963CLzW+rspHr+eyiHNDfAMdbwIDAQAB";
            string keySignature = "Dt2GPh6kq+rP+44lS6UQZtc23XlM/nSwNWOdykACiRQ64j1FSTYYYugzJk5pMIJjFmqc9qAAgStD8mdpYx3p2Xkt8P9YdFA/f0UNkeCfvK/XYQ0MJfN0p4YmaZqKbJCmeLf4R1iL+4Qf+TA6DjtHn+8vEum/A4yWs4aiZLHD7zk=";
            string keyExpire = "2018/02/27 10-01-28";
            string data = "1638_270328,525";

            VerifyResult result = Verifier.VerifyUserquake(data, dataExpire, dataSignature, publicKey, keySignature, keyExpire, DateTime.Parse("2018/02/27 03:29:32"));
            Assert.IsFalse(result.isExpired);
            Assert.IsFalse(result.isValidSignature);
        }

        [TestCase]
        public void VerifyUserquake_CorruptedDataSignature()
        {
            string dataSignature = "SIGNATURE_CORRUPTED";
            string dataExpire = "2018/02/27 03-29-32";
            string publicKey = "MEowDQYJKoZIhvcNAQEBBQADOQAwNgIxAOhqFvL7N/G2RlrluixV6efWsAaAA+9albM86Ltv9MJjwLBiJYAXZPI9UJY++COYOwIBEQ==";
            string keySignature = "SMCL2D6mR+3LWCa3A5a+j5m3+6G1Utt0zcvVMQTiBhI657RG0n008rNAEuAF02YhA12cNhT6Rji9op7dmPNQhRGDvLCeYQPJuA482Q42Ld4iH9CPNUDDxOJMKN/hJC8LSb0cL+Q7RaCml/+J07cI9VG4nHC9f0X2dhQ5790RkKM=";
            string keyExpire = "2018/02/27 04-17-21";
            string data = "1638_270328,525";

            VerifyResult result = Verifier.VerifyUserquake(data, dataExpire, dataSignature, publicKey, keySignature, keyExpire, DateTime.Parse("2018/02/27 03:29:32"));
            Assert.IsFalse(result.isExpired);
            Assert.IsFalse(result.isValidSignature);
        }

        [TestCase]
        public void VerifyUserquake_CorruptedKeySignature()
        {
            string dataSignature = "0C/bVWbyqNRCbOijje6ihB/Xk+gcaBiBj8oN13kpP8n9CbesxIidbLp1EzcVAa/C";
            string dataExpire = "2018/02/27 03-29-32";
            string publicKey = "MEowDQYJKoZIhvcNAQEBBQADOQAwNgIxAOhqFvL7N/G2RlrluixV6efWsAaAA+9albM86Ltv9MJjwLBiJYAXZPI9UJY++COYOwIBEQ==";
            string keySignature = "SIGNATURE_CORRUPTED";
            string keyExpire = "2018/02/27 04-17-21";
            string data = "1638_270328,525";

            VerifyResult result = Verifier.VerifyUserquake(data, dataExpire, dataSignature, publicKey, keySignature, keyExpire, DateTime.Parse("2018/02/27 03:29:32"));
            Assert.IsFalse(result.isExpired);
            Assert.IsFalse(result.isValidSignature);
        }

        [TestCase]
        public void VerifyUserquake_CorruptedPublicKey()
        {
            string dataSignature = "0C/bVWbyqNRCbOijje6ihB/Xk+gcaBiBj8oN13kpP8n9CbesxIidbLp1EzcVAa/C";
            string dataExpire = "2018/02/27 03-29-32";
            string publicKey = "KEY_CORRUPTED";
            string keySignature = "SMCL2D6mR+3LWCa3A5a+j5m3+6G1Utt0zcvVMQTiBhI657RG0n008rNAEuAF02YhA12cNhT6Rji9op7dmPNQhRGDvLCeYQPJuA482Q42Ld4iH9CPNUDDxOJMKN/hJC8LSb0cL+Q7RaCml/+J07cI9VG4nHC9f0X2dhQ5790RkKM=";
            string keyExpire = "2018/02/27 04-17-21";
            string data = "1638_270328,525";

            VerifyResult result = Verifier.VerifyUserquake(data, dataExpire, dataSignature, publicKey, keySignature, keyExpire, DateTime.Parse("2018/02/27 03:29:32"));
            Assert.IsFalse(result.isExpired);
            Assert.IsFalse(result.isValidSignature);
        }

        [TestCase]
        public void VerifyUserquake_CorruptedDataExpire()
        {
            string dataSignature = "0C/bVWbyqNRCbOijje6ihB/Xk+gcaBiBj8oN13kpP8n9CbesxIidbLp1EzcVAa/C";
            string dataExpire = "EXPIRE_CORRUPTED";
            string publicKey = "MEowDQYJKoZIhvcNAQEBBQADOQAwNgIxAOhqFvL7N/G2RlrluixV6efWsAaAA+9albM86Ltv9MJjwLBiJYAXZPI9UJY++COYOwIBEQ==";
            string keySignature = "SMCL2D6mR+3LWCa3A5a+j5m3+6G1Utt0zcvVMQTiBhI657RG0n008rNAEuAF02YhA12cNhT6Rji9op7dmPNQhRGDvLCeYQPJuA482Q42Ld4iH9CPNUDDxOJMKN/hJC8LSb0cL+Q7RaCml/+J07cI9VG4nHC9f0X2dhQ5790RkKM=";
            string keyExpire = "2018/02/27 04-17-21";
            string data = "1638_270328,525";

            VerifyResult result = Verifier.VerifyUserquake(data, dataExpire, dataSignature, publicKey, keySignature, keyExpire, DateTime.Parse("2018/02/27 03:29:32"));
            Assert.IsTrue(result.isExpired);
            Assert.IsFalse(result.isValidSignature);
        }

        [TestCase]
        public void VerifyUserquake_CorruptedKeyExpire()
        {
            string dataSignature = "0C/bVWbyqNRCbOijje6ihB/Xk+gcaBiBj8oN13kpP8n9CbesxIidbLp1EzcVAa/C";
            string dataExpire = "2018/02/27 03-29-32";
            string publicKey = "MEowDQYJKoZIhvcNAQEBBQADOQAwNgIxAOhqFvL7N/G2RlrluixV6efWsAaAA+9albM86Ltv9MJjwLBiJYAXZPI9UJY++COYOwIBEQ==";
            string keySignature = "SMCL2D6mR+3LWCa3A5a+j5m3+6G1Utt0zcvVMQTiBhI657RG0n008rNAEuAF02YhA12cNhT6Rji9op7dmPNQhRGDvLCeYQPJuA482Q42Ld4iH9CPNUDDxOJMKN/hJC8LSb0cL+Q7RaCml/+J07cI9VG4nHC9f0X2dhQ5790RkKM=";
            string keyExpire = "EXPIRE_CORRUPTED";
            string data = "1638_270328,525";

            VerifyResult result = Verifier.VerifyUserquake(data, dataExpire, dataSignature, publicKey, keySignature, keyExpire, DateTime.Parse("2018/02/27 03:29:32"));
            Assert.IsTrue(result.isExpired);
            Assert.IsFalse(result.isValidSignature);
        }
    }
}
