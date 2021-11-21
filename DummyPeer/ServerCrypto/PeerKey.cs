using Asn1PKCS.Encoder;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DummyPeer.ServerCrypto
{
    public class PeerKey
    {
        public DateTime Expire { get; set; }
        public string KeySignature { get; set; }
        public RSAParameters RSAParam { get; set; }

        /// <summary>
        /// PKCS #8 RSAPublicKey
        /// </summary>
        public string PublicKey
        {
            get { return PKCS8DEREncoder.EncodePublicKeyToBase64(RSAParam); }
        }

        /// <summary>
        /// PKCS #8 RSAPublicKey
        /// </summary>
        public byte[] PublicKeyBytes
        {
            get { return PKCS8DEREncoder.EncodePublicKey(RSAParam); }
        }

        /// <summary>
        /// PKCS #8 RSAPrivateKey
        /// </summary>
        public string PrivateKey
        {
            get { return PKCS8DEREncoder.EncodePrivateKeyToBase64(RSAParam); }
        }
    }
}
