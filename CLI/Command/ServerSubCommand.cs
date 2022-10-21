using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Layout;

using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Net;
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

                    response.StatusCode = 200;
                    response.Close();
                }
                catch (Exception e)
                {
                    logger.Error("Exception occured!", e);
                }
            }
        }

        public void Abort()
        {
            quit = true;
        }
    }
}
