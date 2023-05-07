using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfClient.Utils
{
    public class KanaConverter
    {
        public static KanaConverter Instance { get; private set; } = new();

        private readonly IReadOnlyDictionary<string, string> point2Kanas;

        private KanaConverter() {
            var kanas = Resource.Points2Kana
                .Split('\n')
                .Where((line) => line.Length > 0)
                .Select((line) => line.Split(','))
                .ToDictionary(e => e[0], e => e[1]);
            point2Kanas = kanas;
        }

        public string GetKana(string name)
        {
            if (point2Kanas.ContainsKey(name))
            {
                return point2Kanas[name];
            }

            return "";
        }
    }
}
