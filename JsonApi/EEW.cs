using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonApi
{
    public class EEW : BasicData
    {
        public bool Cancelled { get; set; }
        public EEWEarthquake Earthquake { get; set; }
        public EEWIssue Issue { get; set; }
        public EEWArea[] Areas { get; set; }
    }

    public class EEWEarthquake
    {
        public string OriginTime { get; set; }
        public string ArrivalTime { get; set; }
        public string Condition { get; set; }
        public EEWHypocenter Hypocenter { get; set; }
    }

    public class EEWIssue
    {
        public string Time { get; set; }
        public string EventID { get; set; }
        public string Serial { get; set; }
    }

    public class EEWHypocenter
    {
        public string Name { get; set; }
        public string ReduceName { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int Depth { get; set; }
        public double Magnitude { get; set; }
    }

    public class EEWArea
    {
        public string Pref { get; set; }
        public string Name { get; set; }
        public int ScaleFrom { get; set; }
        public int ScaleTo { get; set; }
        public string KindCode { get; set; }
        public string ArrivalTime { get; set; }
    }
}
