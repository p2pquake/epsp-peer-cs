using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Misc.UQSummary
{
    public class UQManager : IUQManager
    {
        private const int Interval = 30;

        public event EventHandler Occurred = (s, e) => { };

        public Func<IDictionary<string, int>> AreaPeerDictionary { private get; set; }
        public Func<DateTime> ProtocolTime { private get; set; }

        public bool IsOnGoing
        {
            get
            {
                if (isOnGoing && ProtocolTime().Subtract(userquakeList.Last().AbstractTime).TotalSeconds >= Interval)
                {
                    isOnGoing = false;
                }
                return isOnGoing;
            }
        }
        public IUQJudge UQJudge { private get; set; }

        private List<Userquake> userquakeList = new List<Userquake>();
        private bool isOnGoing = false;

        public void Add(string areaCode)
        {
            var userquake = new Userquake();
            userquake.AreaCode = areaCode;
            userquake.AbstractTime = ProtocolTime();
            userquake.RelativeTime = new TimeSpan(0);

            if (userquakeList.Count > 0 &&
                userquake.AbstractTime.Subtract(userquakeList.Last().AbstractTime).TotalSeconds >= Interval)
            {
                userquakeList.Clear();
                isOnGoing = false;
            }

            if (userquakeList.Count > 0)
            {
                userquake.RelativeTime = userquake.AbstractTime.Subtract(userquakeList.First().AbstractTime);
            }

            userquakeList.Add(userquake);

            if (!IsOnGoing && UQJudge.Judge(userquakeList))
            {
                isOnGoing = true;
                Occurred(this, EventArgs.Empty);
            }
        }

        public IDictionary<string, int> GetCurrentSummary()
        {
            return userquakeList.GroupBy(e => e.AreaCode).ToDictionary(e => e.Key, e => e.Count());
        }
    }
}
