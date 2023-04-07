using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace JsonApi
{
    public enum Code
    {
        Earthquake = 551,
        Tsunami = 552,
        [Obsolete("代わりに EEW を使用してください。")]
        EEWTest = 554,
        Areapeers = 555,
        EEW = 556,
        Userquake = 561,
        UserquakeEvaluation = 9611,
    }

    public class Client
    {
        static readonly HttpClientHandler handler = new HttpClientHandler()
        {
            AutomaticDecompression = System.Net.DecompressionMethods.All,
        };
        static readonly HttpClient client = new(handler);

        public async static Task<BasicData[]> Get(int limit = 100, int offset = 0, params Code[] codes)
        {
            var response = await client.GetAsync($"https://api.p2pquake.net/v2/history?limit={limit}&offset={offset}{(codes.Length > 0 ? "&" : "")}{string.Join('&', codes.Select(e => $"codes={(int)e}"))}");
            response.EnsureSuccessStatusCode();
            string body = await response.Content.ReadAsStringAsync();

            var deserializeOptions = new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };
            var items = JsonSerializer.Deserialize<BasicData[]>(body, deserializeOptions);

            return items;
        }
    }
}
