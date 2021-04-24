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
            mc.Connect();

            Console.ReadLine();
        }
    }
}
