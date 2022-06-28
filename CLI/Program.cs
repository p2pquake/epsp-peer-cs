using CLI.Command;
using CLI.Observers;

using Client.App;

using log4net.Config;

using System;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace CLI
{
    class Program
    {
        static int Main(string[] args)
        {
            var root = new RootCommand()
            {
                MapCommand.Build(),
                LoggerCommand.Build(),
                PublisherCommand.Build(),
                new System.CommandLine.Command("legacy", "これまでの Observers CLI を起動します")
                {
                    Handler = CommandHandler.Create(() =>
                    {
                        GrcpMain();
                    })
                },
            };

            return root.InvokeAsync(args).Result;
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
