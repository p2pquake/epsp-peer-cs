using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace JsonApi
{
    [JsonConverter(typeof(JsonApiConverterWithTypeDiscriminator))]
    public class BasicData
    {
        public string Id { get; set; }
        public int Code { get; set; }
        public string Time { get; set; }
    }
}
