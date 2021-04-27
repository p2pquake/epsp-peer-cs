using CLI.Receiver;

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
            var dr = ReceiverFactory.CreateReceiver(ReceiverType.Dummy, mc);
            var pr = ReceiverFactory.CreateReceiver(ReceiverType.Print, mc);
            mc.Connect();

            Console.ReadLine();
        }
    }
}
