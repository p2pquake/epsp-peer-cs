using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace AvaloniaUIClient.Utils
{
    public static class AreaHelper
    {
        private static Dictionary<string, string>? _areas;

        public static Dictionary<string, string> Areas
        {
            get
            {
                if (_areas == null)
                {
                    LoadAreas();
                }
                return _areas!;
            }
        }

        private static void LoadAreas()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = "AvaloniaUIClient.Resources.epsp-area.csv";
                
                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream == null)
                {
                    _areas = new Dictionary<string, string>();
                    return;
                }

                using var reader = new StreamReader(stream);
                var csv = reader.ReadToEnd();
                
                _areas = csv.Split('\n')
                    .Skip(1) // ヘッダーをスキップ
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .Select(line => line.Split(','))
                    .Where(parts => parts.Length >= 5)
                    .ToDictionary(parts => parts[0], parts => parts[4]); // 地域コード → 地域名
            }
            catch (Exception)
            {
                _areas = new Dictionary<string, string>();
            }
        }

        public static string GetAreaName(string areaCode)
        {
            return Areas.TryGetValue(areaCode, out var name) ? name : $"地域{areaCode}";
        }
    }
}