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

    // XXX: できれば共通化したい。
    public class MultiLineGeoJson
    {
        public string type { get; set; }
        public MultiLineFeature[] features { get; set; }
    }

    public class MultiLineFeature
    {
        public string type { get; set; }
        public MultiLineGeometry geometry { get; set; }
        public MultiLineProperties properties { get; set; }
    }

    public class MultiLineGeometry
    {
        public string type { get; set; }
        public float[][][] coordinates { get; set; }
    }

    public class MultiLineProperties
    {
        public string code { get; set; }
    }
}
