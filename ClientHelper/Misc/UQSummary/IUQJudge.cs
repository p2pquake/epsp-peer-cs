using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientHelper.Misc.UQSummary
{
    /// <summary>地震感知情報の表示判定を行うインタフェース</summary>
    public interface IUQJudge
    {
        /// <summary>地震感知情報の表示しきい値に達したかどうか判定します。</summary>
        /// <param name="userquakeList">地震感知情報（受信日時の昇順でソートされていること）</param>
        /// <param name="areaPeerDictionary">地域ピア数</param>
        /// <returns>true: 表示しきい値に達した</returns>
        bool Judge(List<Userquake> userquakeList, Func<IDictionary<string, int>> areaPeerDictionary);
    }
}
