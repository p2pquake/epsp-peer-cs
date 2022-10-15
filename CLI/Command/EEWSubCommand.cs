using Map.Controller;
using Map.Model;

using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;

namespace CLI.Command
{
    public class EEWSubCommand
    {
        public static System.CommandLine.Command Build()
        {
            var command = new System.CommandLine.Command(
                "eew",
                "緊急地震速報の地図を生成します"
            )
            {
                new Option<string>("--output", () => "output.png"),
                new Option<bool>("--trim", () => true),
                new Option<MapType>("--map-type", () => MapType.JAPAN_1024),
                new Option<double?>("--latitude"),
                new Option<double?>("--longitude"),
                new Option<string>("--areas-file"),
            };

            command.Handler = CommandHandler.Create<EEWOptions>(EEWHandler);
            return command;
        }

        public class EEWOptions
        {
            public string Output { get; set; }
            public bool Trim { get; set; }
            public MapType MapType { get; set; }
            public double? Latitude { get; set; }
            public double? Longitude { get; set; }
            public string AreasFile { get; set; }
        }

        private static void EEWHandler(EEWOptions options)
        {
            if (options.Latitude.HasValue != options.Longitude.HasValue)
            {
                Console.Error.WriteLine("Latitude (--latitude) and Longitude (--longitude) must be specified at the same time.");
                return;
            }

            var hypocenter = (options.Latitude.HasValue && options.Longitude.HasValue) ? new GeoCoordinate(options.Latitude.Value, options.Longitude.Value) : null;

            var eewPoints = File.ReadAllLines(options.AreasFile).Distinct().Select((line) => new EEWPoint(EEWConverter.GetAreaCode(line).ToString())).ToArray();

            var drawer = new MapDrawer()
            {
                MapType = options.MapType,
                Trim = options.Trim,
                Hypocenter = hypocenter,
                EEWPoints = eewPoints,
            };

            var png = drawer.DrawAsPng();
            using var file = File.OpenWrite(options.Output);
            png.CopyTo(file);
            file.Close();
            png.Close();
        }
    }
}
