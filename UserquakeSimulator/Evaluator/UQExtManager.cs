using Client.Misc.UQSummary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserquakeSimulator.Evaluator
{
    public class UQExtManager : UQManager, IUQExtManager
    {
        public event EventHandler<UQDataEventArgs> Initialized;
        private TimeSpan elapsed;

        public UQExtManager()
        {
            Interval = 40;
            elapsed = TimeSpan.MinValue;
        }

        public override void Add(string areaCode)
        {
            if (userquakeList.Count > 0 &&
                ProtocolTime().Subtract(userquakeList.Last().AbstractTime).TotalSeconds >= Interval)
            {
                var eventArgs = new UQDataEventArgs()
                {
                    List = new List<Userquake>(userquakeList),
                    Summary = GetCurrentSummary(),
                    IsSatisfied = isOnGoing,
                    Elapsed = elapsed
                };
                Initialized(this, eventArgs);
                elapsed = TimeSpan.MinValue;
            }

            base.Add(areaCode);

            if (isOnGoing && elapsed == TimeSpan.MinValue)
            {
                elapsed = userquakeList.Last().RelativeTime;
            }
        }
    }
}
