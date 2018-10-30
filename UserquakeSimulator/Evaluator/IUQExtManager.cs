using Client.Misc.UQSummary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserquakeSimulator.Evaluator
{
    public class UQDataEventArgs : EventArgs
    {
        public IList<Userquake> List { get; set; }
        public IDictionary<string, int> Summary { get; set; }
        public bool IsSatisfied { get; set; }
    }

    public interface IUQExtManager : IUQManager
    {
        event EventHandler<UQDataEventArgs> Initialized;
    }
}
