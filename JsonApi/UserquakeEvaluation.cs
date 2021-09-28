using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace JsonApi
{
    public class UserquakeEvaluation : BasicData
    {
        public int Count { get; set; }
        public float Confidence { get; set; }
        [JsonPropertyName("started_at")]
        public string StartedAt { get; set; }
        [JsonPropertyName("updated_at")]
        public string UpdatedAt { get; set; }
        [JsonPropertyName("area_confidences")]
        public IDictionary<string, AreaConfidence> AreaConfidences { get; set; }
    }

    public class AreaConfidence
    {
        public float Confidence { get; set; }
        public int Count { get; set; }
        public string Display { get; set; }
    }
}
