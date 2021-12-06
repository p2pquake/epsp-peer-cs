using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.App
{
    /// <summary>
    /// EPSPピアの設定を行うインタフェースです。
    /// </summary>
    public interface IPeerConfig
    {
        /// <summary>
        /// 地域コード(3桁)
        /// <para>0埋めのある本来の地域コードは <see cref="FormattedAreaCode"/> を参照してください。</para>
        /// </summary>
        int AreaCode { get; set; }
        /// <summary>地域コード(3桁)</summary>
        string FormattedAreaCode { get; }

        /// <summary>ポート開放するかどうか</summary>
        bool IsPortOpen { get; set; }
        /// <summary>ポートをリッスンしているかどうか</summary>
        bool IsPortListening { get; }
        /// <summary>ポート番号</summary>
        int Port { get; set; }

        /// <summary>最大接続数</summary>
        int MaxConnections { get; set; }
    }
}
