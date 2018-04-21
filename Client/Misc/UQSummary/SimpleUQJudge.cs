using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Misc.UQSummary
{
    class SimpleUQJudge : IUQJudge
    {
        private int thresholdCount;

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
