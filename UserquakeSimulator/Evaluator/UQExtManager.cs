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

        public UQExtManager()
        {
            Interval = 40;
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
                    IsSatisfied = isOnGoing
                };
                Initialized(this, eventArgs);
            }

            base.Add(areaCode);
        }
    }
}
