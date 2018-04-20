using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Misc.UQSummary
{
    public class UQManager : IUQManager
    {
        public event EventHandler Occurred = (s, e) => { };

        public Func<IDictionary<string, int>> AreaPeerDictionary { private get; set; }
        public Func<DateTime> ProtocolTime { private get; set; }

        public bool IsOnGoing { get; private set; }
        public IUQJudge UQJudge { private get; set; }

        private List<Userquake> userquakeList = new List<Userquake>();

        public void Add(string areaCode)
        {
            var userquake = new Userquake();
            userquake.AreaCode = areaCode;
            userquake.AbstractTime = ProtocolTime();
            userquake.RelativeTime = new TimeSpan(0);

            if (userquakeList.Count > 0 &&
                userquake.AbstractTime.Subtract(userquakeList.Last().AbstractTime).TotalSeconds >= 30)
            {
                userquakeList.Clear();
                IsOnGoing = false;
            }

            if (userquakeList.Count > 0)
            {
                userquake.RelativeTime = userquake.AbstractTime.Subtract(userquakeList.First().AbstractTime);
            }

            userquakeList.Add(userquake);

            if (!IsOnGoing && UQJudge.Judge(userquakeList))
            {
                IsOnGoing = true;
                Occurred(this, EventArgs.Empty);
            }
        }

        public IDictionary<string, int> GetCurrentSummary()
        {
            return userquakeList.GroupBy(e => e.AreaCode).ToDictionary(e => e.Key, e => e.Count());
        }
    }
}
