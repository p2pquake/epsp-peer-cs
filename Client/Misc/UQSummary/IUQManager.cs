using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Misc.UQSummary
{
    public class Userquake
    {
        public DateTime AbstractTime { get; set; }
        public TimeSpan RelativeTime { get; set; }
        public string AreaCode { get; set; }
    }

    public interface IUQManager
    {
        event EventHandler Occurred;

        Func<DateTime> ProtocolTime { set; }
        Func<IDictionary<string, int>> AreaPeerDictionary { set; }

        bool IsOnGoing { get; }
        IUQJudge UQJudge { set; }

        void Add(string areaCode);
        IDictionary<string, int> GetCurrentSummary();
    }
}
