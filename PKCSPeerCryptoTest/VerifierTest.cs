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
		public void JMAQuake_Correct()
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
        public void JMAQuake_Expired()
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
        public void JMAQuake_InvalidSignature_Edited()
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
        public void JMAQuake_InvalidSignature_Another()
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
		public void InvalidBase64()
        {
            string signature = "INVALID_SIGNATURE";
            string expire = "2018/02/26 01-39-08";
            string summary = "26日1時28分,4,0,3,福島県沖,40km,5.7,0,N37.5,E141.8,気象庁";
            string detail = "-福島県,+4,*福島県中通り,*福島県浜通り,-宮城県,+3,*宮城県北部,*宮城県南部,*宮城県中部,-福島県,+3,*福島県会津,-茨城県,+3,*茨城県北部,*茨城県南部,-栃木県,+3,*栃木県北部,*栃木県南部,-群馬県,+3,*群馬県南部,-埼玉県,+3,*埼玉県南部,-千葉県,+3,*千葉県北西部";

            VerifyResult result = Verifier.VerifyServerData(summary + detail, expire, signature, DateTime.Parse("2018/02/26 01:39:08"));
            Assert.IsFalse(result.isValidSignature);
        }

        [TestCase]
        public void InvalidSignature()
        {
            string signature = "VEVTVA==";
            string expire = "2018/02/26 01-39-08";
            string summary = "26日1時28分,4,0,3,福島県沖,40km,5.7,0,N37.5,E141.8,気象庁";
            string detail = "-福島県,+4,*福島県中通り,*福島県浜通り,-宮城県,+3,*宮城県北部,*宮城県南部,*宮城県中部,-福島県,+3,*福島県会津,-茨城県,+3,*茨城県北部,*茨城県南部,-栃木県,+3,*栃木県北部,*栃木県南部,-群馬県,+3,*群馬県南部,-埼玉県,+3,*埼玉県南部,-千葉県,+3,*千葉県北西部";

            VerifyResult result = Verifier.VerifyServerData(summary + detail, expire, signature, DateTime.Parse("2018/02/26 01:39:08"));
            Assert.IsFalse(result.isValidSignature);
        }

        [TestCase]
        public void InvalidExpire()
        {
            string signature = "aOOBMtic8+BxJyZqg7ctWmuS77a89rapTY4qqs4/AHbdUDFdT05w2lxEW+arYnAiaLhvSuZF9UcFeDgm3Et7LU2qMRKASLqOJLEDpX5R1TQHq8IEp4M6NfS3UyI6GDcEJw5yxcmfqFjy2nfVqcb4m1LeP8HCsuCJa9ZaLA5IBU0=";
            string expire = "INVALID_DATE";
            string summary = "26日1時28分,4,0,3,福島県沖,40km,5.7,0,N37.5,E141.8,気象庁";
            string detail = "-福島県,+4,*福島県中通り,*福島県浜通り,-宮城県,+3,*宮城県北部,*宮城県南部,*宮城県中部,-福島県,+3,*福島県会津,-茨城県,+3,*茨城県北部,*茨城県南部,-栃木県,+3,*栃木県北部,*栃木県南部,-群馬県,+3,*群馬県南部,-埼玉県,+3,*埼玉県南部,-千葉県,+3,*千葉県北西部";

            VerifyResult result = Verifier.VerifyServerData(summary + detail, expire, signature, DateTime.Parse("2018/02/26 01:39:08"));
            Assert.IsTrue(result.isExpired);
        }
    }
}
