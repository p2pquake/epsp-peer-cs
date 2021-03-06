﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Client.Client.General
{
    /// <summary>
    /// 鍵の情報をあらわすクラス
    /// </summary>
    public class KeyData
    {
        /// <summary>秘密鍵</summary>
        public string PrivateKey { get; set; }
        /// <summary>公開鍵</summary>
        public string PublicKey { get; set; }
        /// <summary>有効期限</summary>
        public DateTime Expire { get; set; }
        /// <summary>署名</summary>
        public string Signature { get; set; }

        public bool IsExpired(DateTime protocolTime)
        {
            return Expire < protocolTime;
        }
    }
}
