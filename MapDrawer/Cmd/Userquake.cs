using Map.Util;

using System.CommandLine;
using System.CommandLine.Invocation;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace MapDrawer.Cmd
{
    class Userquake
    {
        public static Command Generate()
        {
            var command = new Command("userquake") {
                    new Option<string>("--image") { IsRequired = true },
                    new Option<string>("--output") { IsRequired = true },
                    new Option<string>("--areas"),
                    new Option<double>("--left") { IsRequired = true },
                    new Option<double>("--right") { IsRequired = true },
                    new Option<double>("--top") { IsRequired = true },
                    new Option<double>("--bottom") { IsRequired = true },
                    new Option("--trim"),
                    new Option("--mercator")
                    };
            command.Handler = CommandHandler.Create((UserquakeOptions userquakeOptions) =>
            {
                var drawer = new Map.Map.MapDrawer(
                    userquakeOptions.Image,
                    new double[] { userquakeOptions.Top, userquakeOptions.Bottom },
                    new double[] { userquakeOptions.Left, userquakeOptions.Right },
                    userquakeOptions.Mercator
                );

                if (userquakeOptions.Areas != null)
                {
                    StreamReader reader = new StreamReader(userquakeOptions.Areas, Encoding.UTF8);
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] items = line.Split(',');
                        double[] area = UserquakeArea2LatLong.convert(items[0]);

                        if (area == null)
                        {
                            continue;
                        }

                        var confidence = double.Parse(items[1]);
                        if (confidence < 0) { continue; }

                        drawer.drawArea(area[0], area[1], confidence);
                    }
                }

                var bitmap = userquakeOptions.Trim ? drawer.TrimmedBitmap : drawer.Bitmap ;
                bitmap.Save(userquakeOptions.Output, ImageFormat.Png);
            });
            return command;
        }
    }

    class UserquakeOptions
    {
        public string Image { get; set; }
        public string Output { get; set; }
        public string Areas { get; set; }
        public double Left { get; set; }
        public double Right { get; set; }
        public double Top { get; set; }
        public double Bottom { get; set; }
        public bool Trim { get; set; }
        public bool Mercator { get; set; }
    }
}
