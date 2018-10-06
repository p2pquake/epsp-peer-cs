using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserquakeSimulator.Reader
{
    public enum DataType
    {
        Userquake,
        Areapeer
    }

    public class EPSPData
    {
        public DataType DataType { get; set; }
        public DateTime ArrivalTime { get; set; }
        public string AreaCode { get; set; }
        public IDictionary<string, int> PeerMap { get; set; }
    }

}
