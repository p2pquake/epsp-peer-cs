using Map.Util;

using System.CommandLine;
using System.CommandLine.Invocation;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace MapDrawer.Cmd
{
    class Quake
    {
        public static Command Generate()
        {
            var command = new Command("quake") {
                    new Option<string>("--image") { IsRequired = true },
                    new Option<string>("--output") { IsRequired = true },
                    new Option<string>("--points"),
                    new Option<double>("--left") { IsRequired = true },
                    new Option<double>("--right") { IsRequired = true },
                    new Option<double>("--top") { IsRequired = true },
                    new Option<double>("--bottom") { IsRequired = true },
                    new Option<double>("--latitude"),
                    new Option<double>("--longitude"),
                    new Option("--trim"),
                    new Option("--mercator")
                    };
            command.Handler = CommandHandler.Create((QuakeOptions quakeOptions) =>
            {
                var drawer = new Map.Map.MapDrawer(
                    quakeOptions.Image,
                    new double[] { quakeOptions.Left, quakeOptions.Right },
                    new double[] { quakeOptions.Top, quakeOptions.Bottom },
                    quakeOptions.Mercator
                );

                if (quakeOptions.Points != null)
                {
                    StreamReader reader = new StreamReader(quakeOptions.Points, Encoding.UTF8);
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] items = line.Split(',');
                        double[] point = PointName2LatLong.convert(items[0]);

                        if (point == null)
                        {
                            continue;
                        }

                        drawer.drawPoint(point[0], point[1], int.Parse(items[1]));
                    }
                }

                if (quakeOptions.Latitude != 0 || quakeOptions.Longitude != 0)
                {
                    drawer.drawHypocenter(quakeOptions.Latitude, quakeOptions.Longitude);
                }

                var bitmap = quakeOptions.Trim ? drawer.TrimmedBitmap : drawer.Bitmap ;
                bitmap.Save(quakeOptions.Output, ImageFormat.Png);
            });
            return command;
        }
    }

    class QuakeOptions
    {
        public string Image { get; set; }
        public string Output { get; set; }
        public string Points { get; set; }
        public double Left { get; set; }
        public double Right { get; set; }
        public double Top { get; set; }
        public double Bottom { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public bool Trim { get; set; }
        public bool Mercator { get; set; }
    }
}
