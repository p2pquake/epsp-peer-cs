using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Misc.UQSummary
{
    public interface IUQJudge
    {
        bool Judge(List<Userquake> userquakeList);
    }
}
