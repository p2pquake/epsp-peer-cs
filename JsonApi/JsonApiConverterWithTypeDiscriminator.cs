using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace JsonApi
{
    public class JsonApiConverterWithTypeDiscriminator : JsonConverter<BasicData>
    {
        public override BasicData Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            using (var document = JsonDocument.ParseValue(ref reader))
            {
                if (!document.RootElement.TryGetProperty("code", out var codeProperty))
                {
                    throw new JsonException();
                }

                var code = codeProperty.GetInt32();
                var rawText = document.RootElement.GetRawText();

                return code switch
                {
                    551 => JsonSerializer.Deserialize<JMAQuake>(rawText, options),
                    552 => JsonSerializer.Deserialize<JMATsunami>(rawText, options),
                    554 => JsonSerializer.Deserialize<EEWDetection>(rawText, options),
                    556 => JsonSerializer.Deserialize<EEW>(rawText, options),
                    9611 => JsonSerializer.Deserialize<UserquakeEvaluation>(rawText, options),
                    _ => JsonSerializer.Deserialize<UnknownData>(rawText, options),
                };
            }
        }

        public override void Write(Utf8JsonWriter writer, BasicData value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
