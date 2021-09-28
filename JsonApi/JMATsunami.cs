using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonApi
{
    public class JMATsunami : BasicData
    {
        public bool Cancelled { get; set; }
        public TsunamiIssue Issue { get; set; }
        public Area[] Areas { get; set; }
    }

    public class TsunamiIssue
    {
        public string Source { get; set; }
        public string Time { get; set; }
        public string Type { get; set; }
    }

    public class Area
    {
        public string Grade { get; set; }
        public bool Immediate { get; set; }
        public string Name { get; set; }
    }

}
