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
    public class UserquakeSubCommand
    {
        public static System.CommandLine.Command Build()
        {
            var command = new System.CommandLine.Command(
                "userquake",
                "地震感知情報の地図を生成します"
                )
            {
                new Option<string>("--output", () => "output.png"),
                new Option<bool>("--trim", () => true),
                new Option<MapType>("--map-type", () => MapType.JAPAN_1024),
                new Option<string[]>("--areacode") { IsRequired = true },
                new Option<double[]>("--confidence") { IsRequired = true },
            };

            command.Handler = CommandHandler.Create<UserquakeOptions>(UserquakeHandler);

            return command;
        }

        public class UserquakeOptions
        {
            public string Output { get; set; }
            public bool Trim { get; set; }
            public MapType MapType { get; set; }
            public string[] Areacode { get; set; }
            public double[] Confidence { get; set; }
        }

        private static void UserquakeHandler(UserquakeOptions options)
        {
            if (options.Areacode.Length != options.Confidence.Length)
            {
                Console.Error.WriteLine("The number of elements in Areacode (--areacode) and Confidence (--confidence) must be the same.");
                return;
            }

            var userquakePoints = options.Areacode.Select((areacode, index) => new UserquakePoint(areacode, options.Confidence[index])).ToArray();

            var drawer = new MapDrawer()
            {
                MapType = options.MapType,
                Trim = options.Trim,
                UserquakePoints = userquakePoints,
            };

            var png = drawer.DrawAsPng();
            using var file = File.OpenWrite(options.Output);
            png.CopyTo(file);
            file.Close();
            png.Close();
        }

    }
}
