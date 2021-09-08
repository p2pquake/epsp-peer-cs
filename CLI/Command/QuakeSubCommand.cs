using Map.Controller;
using Map.Model;

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CLI.Command
{
    public class QuakeSubCommand
    {
        public static System.CommandLine.Command Build()
        {
            var command = new System.CommandLine.Command(
                "quake",
                "地震情報の地図を生成します"
                )
            {
                new Option<string>("--output", () => "output.png"),
                new Option<bool>("--trim", () => true),
                new Option<MapType>("--map-type", () => MapType.JAPAN_1024),
                new Option<double?>("--latitude"),
                new Option<double?>("--longitude"),
                new Option<string[]>(new[]{ "-p", "--pref", "--prefecture" }, () => Array.Empty<string>()),
                new Option<string[]>(new[]{ "-n", "--name" }, () => Array.Empty<string>()),
                new Option<int[]>(new[]{ "-s", "--scale" }, () => Array.Empty<int>()),
            };

            command.Handler = CommandHandler.Create<QuakeOptions>(QuakeHandler);

            return command;
        }

        public class QuakeOptions
        {
            public string Output { get; set; }
            public bool Trim { get; set; }
            public MapType MapType { get; set; }
            public double? Latitude { get; set; }
            public double? Longitude { get; set; }
            public string[] Prefecture { get; set; }
            public string[] Name { get; set; }
            public int[] Scale { get; set; }
        }

        private static void QuakeHandler(QuakeOptions options)
        {
            if (options.Prefecture.Length != options.Name.Length || options.Name.Length != options.Scale.Length)
            {
                Console.Error.WriteLine("The number of elements in Prefecture (--prefecture), Name (--name) and Scale (--scale) must be the same.");
                return;
            }

            var hypocenter = (options.Latitude.HasValue && options.Longitude.HasValue) ? new GeoCoordinate(options.Latitude.Value, options.Longitude.Value) : null;
            var observationPoints = options.Prefecture.Select((prefecture, index) => new ObservationPoint(prefecture, options.Name[index], options.Scale[index])).ToArray();

            var drawer = new MapDrawer()
            {
                MapType = options.MapType,
                Trim = options.Trim,
                Hypocenter = hypocenter,
                ObservationPoints = observationPoints,
            };

            var png = drawer.DrawAsPng();
            using var file = File.OpenWrite(options.Output);
            png.CopyTo(file);
            file.Close();
            png.Close();
        }
    }
}
