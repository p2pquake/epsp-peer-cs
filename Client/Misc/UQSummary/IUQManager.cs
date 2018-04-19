using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Misc.UQSummary
{
    public class Userquake
    {
        DateTime AbstractTime { get; set; }
        TimeSpan RelativeTime { get; set; }
        string AreaCode { get; set; }
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
