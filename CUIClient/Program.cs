using System;

namespace CUIClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var runner = new ClientRunner();
            runner.Start(6911);
            
            Console.ReadLine();
            runner.Stop();
        }
    }
}
