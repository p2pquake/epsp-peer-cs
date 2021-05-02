using CLI.Observers;

using Client.App;

using log4net.Config;

using System;

namespace CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            BasicConfigurator.Configure();

            var mc = new MediatorContext();
            var dr = ObserverFactory.CreateReceiver(ObserverType.Dummy, mc);
            var pr = ObserverFactory.CreateReceiver(ObserverType.Print, mc);
            var gr = ObserverFactory.CreateReceiver(ObserverType.Grpc, mc);
            mc.Connect();

            Console.ReadLine();
        }
    }
}
