using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonApi
{

    public class JMAQuake : BasicData
    {
        public QuakeIssue Issue { get; set; }
        public Earthquake Earthquake { get; set; }
        public Point[] Points { get; set; }
        public Comments Comments { get; set; }
    }

    public class QuakeIssue
    {
        public string Correct { get; set; }
        public string Source { get; set; }
        public string Time { get; set; }
        public string Type { get; set; }
    }

    public class Earthquake
    {
        public string Time { get; set; }
        public Hypocenter Hypocenter { get; set; }
        public int MaxScale { get; set; }
        public string DomesticTsunami { get; set; }
        public string ForeignTsunami { get; set; }
    }

    public class Hypocenter
    {
        public string Name { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public int Depth { get; set; }
        public float Magnitude { get; set; }
    }

    public class Point
    {
        public string Pref { get; set; }
        public string Addr { get; set; }
        public bool IsArea { get; set; }
        public int Scale { get; set; }
    }

    public class Comments
    {
        public string FreeFormComment { get; set; }
    }
}
