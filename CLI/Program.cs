using CLI.Command;
using CLI.Command;
using CLI.Observers;

using Client.App;

using log4net.Config;

using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.Threading;

namespace CLI
{
    class Program
    {
        static int Main(string[] args)
        {
            var root = new RootCommand()
            {
                MapCommand.Build(),
                new System.CommandLine.Command("legacy", "これまでの Observers CLI を起動します")
                {
                    Handler = CommandHandler.Create(() =>
                    {
                        GrcpMain();
                    })
                },
                new System.CommandLine.Command("run", "P2P地震情報 ピアとして動作します")
                {
                    Handler = CommandHandler.Create(() =>
                    {
                        Run();
                    })
                },
            };

            return root.InvokeAsync(args).Result;
        }

        private static void Run()
        {
            BasicConfigurator.Configure();

            var mc = new MediatorContext();
            mc.Connect();

            Console.WriteLine("Enter キーを押すと終了します。");
            Console.ReadLine();

            if (mc.CanDisconnect)
            {
                mc.Disconnect();
            }

            var sw = new Stopwatch();
            sw.Start();

            while (sw.ElapsedMilliseconds <= 4000 && !mc.CanConnect)
            {
                Thread.Sleep(250);
            }
        }

        static void GrcpMain()
        {
            BasicConfigurator.Configure();

            var mc = new MediatorContext();
            var dr = ObserverFactory.CreateObserver(ObserverType.Dummy, mc);
            var pr = ObserverFactory.CreateObserver(ObserverType.Print, mc);
            var gr = ObserverFactory.CreateObserver(ObserverType.Grpc, mc);

            Console.ReadLine();
        }
    }
}
