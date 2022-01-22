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
    public class TsunamiSubCommand
    {
        public static System.CommandLine.Command Build()
        {
            var command = new System.CommandLine.Command(
                "tsunami",
                "津波予報の地図を生成します"
                )
            {
                new Option<string>("--output", () => "output.png"),
                new Option<bool>("--trim", () => true),
                new Option<MapType>("--map-type", () => MapType.JAPAN_1024),
                new Option<string[]>(new[]{"-c", "--category" }, () => Array.Empty<string>()),
                new Option<string[]>(new[]{"-r", "--region" }, () => Array.Empty<string>()),
                new Option<string>("--regions-file"),
                new Option<bool>("--draw", () => true),
            };

            command.Handler = CommandHandler.Create<TsunamiOptions>(TsunamiHandler);

            return command;
        }

        public class TsunamiOptions
        {
            public string Output { get; set; }
            public bool Trim { get; set; }
            public MapType MapType { get; set; }
            public TsunamiCategory[] Category { get; set; }
            public string[] Region { get; set; }
            public string RegionsFile { get; set; }
            public bool Draw { get; set; }
        }

        private static TsunamiCategory GetTsunamiCategory(string category)
        {
            if (category == "Advisory" || category == "津波注意報")
            {
                return TsunamiCategory.Advisory;
            }
            if (category == "Warning" || category == "津波警報")
            {
                return TsunamiCategory.Warning;
            }
            if (category == "MajorWarning" || category == "大津波警報")
            {
                return TsunamiCategory.MajorWarning;
            }
            return TsunamiCategory.Unknown;
        }

        private static void TsunamiHandler(TsunamiOptions options)
        {
            if (options.Category.Length != options.Region.Length)
            {
                Console.Error.WriteLine("The number of elements in Category (--category) and Region (--region) must be the same.");
                return;
            }

            if (options.Category.Length <= 0 && options.RegionsFile == null)
            {
                Console.Error.WriteLine("Please specify forecast regions (--category, --region) or regions file (--regions-file).");
                return;
            }

            var tsunamiPoints = options.Region.Select((region, index) => new TsunamiPoint(region, options.Category[index])).ToArray();
            if (options.RegionsFile != null)
            {
                tsunamiPoints = File.ReadAllLines(options.RegionsFile).Select((line) => line.Split(',')).Select((items) => new TsunamiPoint(items[0], GetTsunamiCategory(items[1]))).ToArray();
            }

            var drawer = new MapDrawer()
            {
                MapType = options.MapType,
                Trim = options.Trim,
                HideDraw = !options.Draw,
                TsunamiPoints = tsunamiPoints,
            };

            var png = drawer.DrawAsPng();
            using var file = File.OpenWrite(options.Output);
            png.CopyTo(file);
            file.Close();
            png.Close();
        }
    }
}
