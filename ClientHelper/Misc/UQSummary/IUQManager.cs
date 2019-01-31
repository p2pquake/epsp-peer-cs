using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientHelper.Misc.UQSummary
{ 
    /// <summary>地震感知情報を表すクラスです。</summary>
    public class Userquake
    {
        /// <summary>絶対日時</summary>
        public DateTime AbstractTime { get; set; }
        /// <summary>地震感知情報1件目からの経過時間</summary>
        public TimeSpan RelativeTime { get; set; }
        /// <summary>地域コード</summary>
        public string AreaCode { get; set; }
    }

    /// <summary>地震感知情報の管理を支援するインタフェースです。</summary>
    public interface IUQManager
    {
        /// <summary>地震感知情報が表示しきい値に達すると発生します。情報は <see cref="GetCurrentSummary"/> で取得します。</summary>
        event EventHandler Occurred;
        /// <summary>地震感知情報が表示しきい値に達した後、情報が更新されると発生します。</summary>
        event EventHandler Updated;

        /// <summary>プロトコル日時を指定します。表示判定に使用します。</summary>
        Func<DateTime> ProtocolTime { set; }
        /// <summary>地域ピア数を指定します。表示判定に使用します。</summary>
        Func<IDictionary<string, int>> AreaPeerDictionary { set; }

        /// <summary>地震感知情報が表示しきい値に達し、イベントが進行中かどうかを表します。</summary>
        bool IsOnGoing { get; }
        /// <summary>地震感知情報の表示判定を行う<see cref="IUQJudge"/>実装を指定します。</summary>
        IUQJudge UQJudge { set; }

        /// <summary>地震感知情報を追加します。</summary>
        /// <param name="areaCode">地域コード</param>
        void Add(string areaCode);
        /// <summary>地震感知情報の集計結果を返します。キーは地域コードです。</summary>
        IDictionary<string, int> GetCurrentSummary();
    }
}
