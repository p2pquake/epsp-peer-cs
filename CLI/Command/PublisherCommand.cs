using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.Threading;
using Client.App;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Layout;

namespace CLI.Command
{
    public class PublisherCommand
    {
        public static System.CommandLine.Command Build()
        {
            var command = new System.CommandLine.Command(
                "publisher",
                "P2P ネットワークに情報発信可能なピアを起動します"
                )
            {
                new Option<int>("--port", () => -1, "プロトコル EPSP に準拠した通信が可能なポート"),
                new Option<int>("--http-port", () => -1, "地域ピア数の取得が可能な HTTP サーバのポート"),
            };
            command.Handler = CommandHandler.Create<PublisherOptions>(PublisherHandler);

            return command;
        }

        public class PublisherOptions
        {
            public int Port { get; set; }
            public int HttpPort { get; set; }
        }

        private static IMediatorContext mc;
        private static ILog logger;

        private static void PublisherHandler(PublisherOptions options)
        {
            var appender = new ConsoleAppender()
            {
                Threshold = log4net.Core.Level.Info,
                Layout = new PatternLayout("%date %-5level %appdomain [%thread]: %message%newline"),
            };
            appender.ActivateOptions();
            BasicConfigurator.Configure(appender);

            logger = LogManager.GetLogger("Publisher");

            mc = new MediatorContext();
            mc.StateChanged += Mc_StateChanged;
            mc.OnData += Mc_OnData;
            mc.IsPortOpen = options.Port > 0;
            mc.Port = options.Port;
            mc.Connect();

            Console.WriteLine("Enter キーを押すと終了します。");
            Console.ReadLine();

            var sw = new Stopwatch();
            sw.Start();
            mc.Disconnect();

            while (sw.ElapsedMilliseconds <= 4000 && !mc.CanConnect)
            {
                Thread.Sleep(250);
            }

        }

        private static void Mc_StateChanged(object sender, EventArgs e)
        {
            logger.Info($"State changed: {mc.ReadonlyState.GetType().Name}");
        }

        private static void Mc_OnData(object sender, Client.Peer.EPSPRawDataEventArgs e)
        {
            logger.Info($"EPSP data arrived: {e.Packet}");
        }
    }
}
