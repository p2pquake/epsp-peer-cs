using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Layout;

using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Net;
using System.Text;
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
            listener.Prefixes.Add("http://localhost:" + port + "/");
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

        private static (int Code, string ContentType, byte[] ResponseBytes) ProcessTest(string _)
        {
            return (200, "text/plain", Encoding.ASCII.GetBytes("OK"));
        }

        private static (int Code, string ContentType, byte[] ResponseBytes) ProcessNotFound(string _)
        {
            return (404, "text/plain", Encoding.ASCII.GetBytes("Function not found"));
        }

        private static (int Code, string ContentType, byte[] ResponseBytes) ProcessEarthquake(string _)
        {
            return (200, "text/plain", Encoding.ASCII.GetBytes("OK"));
        }

        private static (int Code, string ContentType, byte[] ResponseBytes) ProcessTsunami(string _)
        {
            return (200, "text/plain", Encoding.ASCII.GetBytes("OK"));
        }

        private static (int Code, string ContentType, byte[] ResponseBytes) ProcessEEW(string _)
        {
            return (200, "text/plain", Encoding.ASCII.GetBytes("OK"));
        }

        private static (int Code, string ContentType, byte[] ResponseBytes) ProcessUserquake(string _)
        {
            return (200, "text/plain", Encoding.ASCII.GetBytes("OK"));
        }

        public void Abort()
        {
            quit = true;
        }
    }
}
