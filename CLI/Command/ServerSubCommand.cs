using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Layout;

using Map.Controller;
using Map.Model;

using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace CLI.Command
{
    public class ServerSubCommand
    {
        public static System.CommandLine.Command Build()
        {
            var command = new System.CommandLine.Command(
                "server",
                "地図生成サーバーを起動します"
            )
            {
                new Option<int>("--port", () => -1),
            };

            command.Handler = CommandHandler.Create<ServerOptions>(ServerHandler);
            return command;
        }

        public class ServerOptions
        {
            public int Port { get; set; }
        }

        private static ILog logger;

        public static void ServerHandler(ServerOptions options)
        {
            var appender = new ConsoleAppender()
            {
                Threshold = log4net.Core.Level.Info,
                Layout = new PatternLayout("%date %-5level %appdomain [%thread]: %message%newline"),
            };
            appender.ActivateOptions();
            BasicConfigurator.Configure(appender);

            logger = LogManager.GetLogger("MapServer");

            var server = new MapServer(options.Port, logger);
            Task.Run(server.Run);

            Console.WriteLine("Enter キーを押すと終了します。");
            Console.ReadLine();
            Console.WriteLine("サーバーを停止しています...");

            server.Abort();
            Thread.Sleep(5000);
        }
    }

    class MapServer
    {
        private int port;
        private ILog logger;
        private bool quit = false;

        public MapServer(int port, ILog logger)
        {
            this.port = port;
            this.logger = logger;
        }

        public void Run()
        {
            HttpListener listener = new HttpListener();
            // FIXME: 実際の実行時は *: とする。
            listener.Prefixes.Add("http://*:" + port + "/");
            listener.Start();

            logger.Info($"HTTPServer listen {port}.");

            while (!quit)
            {
                try
                {
                    HttpListenerContext context = listener.GetContext();
                    HttpListenerRequest request = context.Request;
                    HttpListenerResponse response = context.Response;

                    var body = "";
                    if (request.HasEntityBody)
                    {
                        using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
                        body = reader.ReadToEnd();
                        request.InputStream.Close();
                        reader.Close();
                    }

                    Func<string, (int Code, string ContentType, byte[] ResponseBytes)> callFunction = request.Url!.AbsolutePath switch
                    {
                        "/test" => ProcessTest,
                        "/earthquake" => ProcessEarthquake,
                        "/tsunami" => ProcessTsunami,
                        "/eew" => ProcessEEW,
                        "/userquake" => ProcessUserquake,
                        _ => ProcessNotFound,
                    };

                    try
                    {
                        if (callFunction == null)
                        {
                            response.StatusCode = 404;
                            response.ContentType = "text/plain";
                            response.OutputStream.Write(Encoding.ASCII.GetBytes("Function not found"));
                        }
                        else
                        {
                            var result = callFunction(body);
                            response.StatusCode = result.Code;
                            response.ContentType = result.ContentType;
                            response.OutputStream.Write(result.ResponseBytes);
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error($"Failed to call function with {request.Url} and {body}", e);
                        response.StatusCode = 500;
                        response.ContentType = "text/plain";
                        response.OutputStream.Write(Encoding.UTF8.GetBytes(e.Message));
                    }

                    response.Close();
                }
                catch (Exception e)
                {
                    logger.Error("Failed to process request", e);
                }
            }
        }

        class QuakeParams
        {
            public bool trim;
            public string mapType;
            public double? latitude;
            public double? longitude;
            public Point[] points;
        }

        class Point
        {
            public string pref;
            public string name;
            public int scale;
        }

        class UserquakeParams
        {
            public bool trim;
            public string mapType;
            public Confidence[] confidences;
        }

        class Confidence
        {
            public string areaCode;
            public double confidence;
        }

        class TsunamiParams
        {
            public bool trim;
            public bool draw;
            public string mapType;
            public Region[] regions;
        }

        class Region
        {
            public string region;
            public string category;
        }

        class EEWParams
        {
            public bool trim;
            public string mapType;
            public double? latitude;
            public double? longitude;
            public Area[] areas;
        }

        class Area
        {
            public string pref;
        }

        private static readonly double aspectRatio = 3.0 / 2.0;

        private static (int Code, string ContentType, byte[] ResponseBytes) ProcessTest(string _)
        {
            return (200, "text/plain", Encoding.ASCII.GetBytes("OK"));
        }

        private static (int Code, string ContentType, byte[] ResponseBytes) ProcessNotFound(string _)
        {
            return (404, "text/plain", Encoding.ASCII.GetBytes("Function not found"));
        }

        private static (int Code, string ContentType, byte[] ResponseBytes) ProcessEarthquake(string body)
        {
            var param = JsonSerializer.Deserialize<QuakeParams>(body, new JsonSerializerOptions { IncludeFields = true });

            var mapType = ToMapType(param.mapType);
            GeoCoordinate hypocenter = null;
            if (param.latitude.HasValue && param.latitude.Value != -1 &&
                param.longitude.HasValue && param.longitude.Value != -1)
                hypocenter = new GeoCoordinate(param.latitude.Value, param.longitude.Value);

            var drawer = new MapDrawer()
            {
                MapType = mapType,
                Trim = param.trim,
                Hypocenter = hypocenter,
                ObservationPoints = param.points.Select(point => new ObservationPoint(point.pref, point.name, point.scale)).ToArray(),
                PreferedAspectRatio = aspectRatio,
            };
            using var png = drawer.DrawAsPng();

            return (200, "image/png", png.ToArray());
        }

        private static (int Code, string ContentType, byte[] ResponseBytes) ProcessTsunami(string body)
        {
            var param = JsonSerializer.Deserialize<TsunamiParams>(body, new JsonSerializerOptions { IncludeFields = true });

            var mapType = ToMapType(param.mapType);

            var drawer = new MapDrawer()
            {
                MapType = mapType,
                Trim = param.trim,
                HideDraw = !param.draw,
                TsunamiPoints = param.regions.Select(region => new TsunamiPoint(region.region, ToTsunamiCategory(region.category))).ToArray(),
                PreferedAspectRatio = aspectRatio,
            };
            using var png = drawer.DrawAsPng();

            return (200, "image/png", png.ToArray());
        }

        private static (int Code, string ContentType, byte[] ResponseBytes) ProcessEEW(string body)
        {
            var param = JsonSerializer.Deserialize<EEWParams>(body, new JsonSerializerOptions { IncludeFields = true });

            var mapType = ToMapType(param.mapType);
            GeoCoordinate hypocenter = null;
            if (param.latitude.HasValue && param.latitude.Value != -1 &&
                param.longitude.HasValue && param.longitude.Value != -1)
                hypocenter = new GeoCoordinate(param.latitude.Value, param.longitude.Value);

            var drawer = new MapDrawer()
            {
                MapType = mapType,
                Trim = param.trim,
                Hypocenter = hypocenter,
                EEWPoints = param.areas.Select(area => area.pref).Distinct().Select(area => new EEWPoint(EEWConverter.GetAreaCode(area).ToString())).ToArray(),
                PreferedAspectRatio = aspectRatio,
            };
            using var png = drawer.DrawAsPng();

            return (200, "image/png", png.ToArray());
        }

        private static (int Code, string ContentType, byte[] ResponseBytes) ProcessUserquake(string body)
        {
            var param = JsonSerializer.Deserialize<UserquakeParams>(body, new JsonSerializerOptions { IncludeFields = true });

            var mapType = ToMapType(param.mapType);

            var drawer = new MapDrawer()
            {
                MapType = mapType,
                Trim = param.trim,
                UserquakePoints = param.confidences.Select(confidence => new UserquakePoint(confidence.areaCode, confidence.confidence)).ToArray(),
                PreferedAspectRatio = aspectRatio,
            };
            using var png = drawer.DrawAsPng();

            return (200, "image/png", png.ToArray());
        }

        public void Abort()
        {
            quit = true;
        }

        private static MapType ToMapType(string mapType)
        {
            return mapType switch
            {
                "JAPAN_1024" => MapType.JAPAN_1024,
                "JAPAN_2048" => MapType.JAPAN_2048,
                "JAPAN_4096" => MapType.JAPAN_4096,
                "JAPAN_8192" => MapType.JAPAN_8192,
                "WORLD_512" => MapType.WORLD_512,
                "WORLD_1024" => MapType.WORLD_1024,
                _ => throw new InvalidOperationException(),
            };
        }

        private static TsunamiCategory ToTsunamiCategory(string category)
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
    }
}
