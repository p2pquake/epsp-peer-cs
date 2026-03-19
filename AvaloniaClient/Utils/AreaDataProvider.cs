using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace AvaloniaClient.Utils;

public static class AreaDataProvider
{
    private static string? cachedCsv;

    public static string EpspAreaCsv
    {
        get
        {
            if (cachedCsv != null) return cachedCsv;

            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = assembly.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith("epsp-area.csv"));
            if (resourceName == null) return "";

            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null) return "";

            using var reader = new StreamReader(stream);
            cachedCsv = reader.ReadToEnd();
            return cachedCsv;
        }
    }

    private static Dictionary<string, string>? areaDictionary;
    public static Dictionary<string, string> AreaDictionary
    {
        get
        {
            areaDictionary ??= EpspAreaCsv.Split('\n').Skip(1).Where(l => l.Contains(',')).Select(e => e.Split(',')).ToDictionary(e => e[0], e => e[4]);
            return areaDictionary;
        }
    }

    private static (int code, string label)[]? areaCodeLabels;
    public static (int code, string label)[] AreaCodeLabels
    {
        get
        {
            areaCodeLabels ??= EpspAreaCsv.Split('\n').Skip(1).Where(l => l.Contains(',')).Select(e => e.Split(',')).Select(e => (int.Parse(e[1]), e[4])).ToArray();
            return areaCodeLabels;
        }
    }

    public static IEnumerable<string> AreaLabels => EpspAreaCsv.Split('\n').Skip(1).Where(l => l.Contains(',')).Select(e => e.Split(',')[4]);
}
