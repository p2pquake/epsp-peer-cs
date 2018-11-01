using Client.Misc.UQSummary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserquakeSimulator.Judge
{
    class OldJudge : IUQJudge
    {
        private int level = 3;

        public OldJudge(int level = 3)
        {
            this.level = level;
        } 

        public bool Judge(List<Userquake> userquakeList, Func<IDictionary<string, int>> areaPeerDictionary)
        {
            if (userquakeList.Count < 3)
            {
                return false;
            }

            var dictionary = areaPeerDictionary();

            var speed = 0.0;
            if (userquakeList.Count >= 2)
            {
                speed = userquakeList.Count / userquakeList.Last().RelativeTime.TotalSeconds;
            }

            var rate = userquakeList.Count / (double)dictionary.Select(e => e.Value).Sum();

            var areaRate =
                userquakeList
                .Where(e => !e.AreaCode.StartsWith("9"))
                .GroupBy(e => e.AreaCode)
                .Where(e => dictionary.ContainsKey(e.Key))
                .Select(e => (e.ToArray().Length / (double)dictionary[e.Key])).DefaultIfEmpty(0).Max();

            var regionRate =
                userquakeList
                .Where(e => !e.AreaCode.StartsWith("9"))
                .GroupBy(e => e.AreaCode[0])
                .Select(e => (e.ToArray().Length / (double)userquakeList.Count)).DefaultIfEmpty(0).Max();

            double factor = new double[] { 0.875, 1.0, 1.2, 1.4 }[level - 1];

            if (speed >= 0.25 * factor && areaRate >= 0.05 * factor)
            {
                return true;
            }
            if (speed >= 0.15 * factor && areaRate >= 0.3 * factor)
            {
                return true;
            }
            if (rate >= 0.01 * factor && areaRate >= 0.035 * factor)
            {
                return true;
            }
            if (rate >= 0.006 * factor && areaRate >= 0.04 * factor && regionRate >= new double[] { 1 * factor, 1 }.Min())
            {
                return true;
            }
            if (speed >= 0.18 * factor && areaRate >= 0.04 * factor && regionRate >= new double[] { 1 * factor, 1 }.Min())
            {
                return true;
            }
            return false;
        }
    }
}
