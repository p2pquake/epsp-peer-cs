namespace Map.Model
{
    public class GeoJson
    {
        public string type { get; set; }
        public Feature[] features { get; set; }
    }

    public class Feature
    {
        public string type { get; set; }
        public Geometry geometry { get; set; }
        public Properties properties { get; set; }
    }

    public class Geometry
    {
        public string type { get; set; }
        public float[][][][] coordinates { get; set; }
    }

    public class Properties
    {
        public string code { get; set; }
        public bool islandBold { get; set; }
        public object name { get; set; }
    }
}
