using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Misc.UQSummary
{
    /// <summary>
    /// 地震感知情報の管理を行う<see cref="IUQManager"/>実装です。
    /// 本実装では、最後の地震感知情報から30秒経過すると別の地震感知情報とみなします。
    /// </summary>
    public class UQManager : IUQManager
    {
        protected int Interval { get; set; } = 30;

        public event EventHandler Occurred = (s, e) => { };
        public event EventHandler Updated = (s, e) => { };

        public Func<IDictionary<string, int>> AreaPeerDictionary { protected get; set; }
        public Func<DateTime> ProtocolTime { protected get; set; }

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

        protected List<Userquake> userquakeList = new List<Userquake>();
        protected bool isOnGoing = false;

        public virtual void Add(string areaCode)
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

            if (IsOnGoing)
            {
                Updated(this, EventArgs.Empty);
            }

            if (!IsOnGoing && UQJudge.Judge(userquakeList, AreaPeerDictionary))
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
