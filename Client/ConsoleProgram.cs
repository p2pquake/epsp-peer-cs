using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Client.App;

namespace Client
{
    class ConsoleProgram
    {
        static void Main(string[] args)
        {
            IOperatable operatableContext = new MediatorContext();

            while (true)
            {
                Console.WriteLine(" 1: 接続");
                Console.WriteLine(" 2: 切断");
                Console.WriteLine(" 3: 維持");
                Console.WriteLine("操作? ");

                string input = Console.ReadLine();

                if (input.StartsWith("1"))
                {
                    operatableContext.Connect();
                }
                else if (input.StartsWith("2"))
                {
                    operatableContext.Disconnect();
                }
                else if (input.StartsWith("3"))
                {
                    ((MediatorContext)operatableContext).Maintain();
                }
            }
        }
    }
}
