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
            var dr = ObserverFactory.CreateObserver(ObserverType.Dummy, mc);
            var pr = ObserverFactory.CreateObserver(ObserverType.Print, mc);
            var gr = ObserverFactory.CreateObserver(ObserverType.Grpc, mc);
            mc.Connect();

            Console.ReadLine();
        }
    }
}
