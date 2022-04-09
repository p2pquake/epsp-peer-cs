using Map.Model;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MapTest
{
    internal static class Internationalization
    {
        public static string[] CultureNames { get; } =
        {
            "ja-JP",
            "en-US",
            "en-GB",
            "fr-FR",
        };

        public static void ReinstantiateSingletonInstances(string cultureName)
        {
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(cultureName);

            var targets = new Dictionary<Type, string>
            {
                { typeof(ObservationAreas), "Instance" },
                { typeof(Stations), "Instance" },
                { typeof(TsunamiAreas), "Instance" },
                { typeof(UserquakeAreas), "Instance" },
            };

            foreach (var target in targets)
            {
                var targetType = target.Key;
                var targetProperty = target.Value;

                var instance = Activator.CreateInstance(targetType, true);
                var property = targetType.GetProperty(targetProperty);
                property?.SetValue(null, instance);
            }
        }
    }
}
