using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientHelper.Misc.UQSummary
{
    /// <summary>地震感知情報の表示判定を件数のみで行う<see cref="IUQJudge"/>実装です。</summary>
    public class SimpleUQJudge : IUQJudge
    {
        private int thresholdCount;

        /// <param name="thresholdCount">表示しきい値となる件数</param>
        public SimpleUQJudge(int thresholdCount = 3)
        {
            this.thresholdCount = thresholdCount;
        }

        public bool Judge(List<Userquake> userquakeList, Func<IDictionary<string, int>> areaPeerDictionary)
        {
            return userquakeList.Count >= thresholdCount;
        }
    }
}
