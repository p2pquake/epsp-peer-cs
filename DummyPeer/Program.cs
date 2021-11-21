using DummyPeer.Peer;

using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Security.Cryptography;

namespace DummyPeer
{
    class RootOptions
    {
        public int Port { get; set; }
    }

    class Program
    {
        static int Main(string[] args)
        {
            var root = new RootCommand()
            {
                new Option<int>(new[]{ "-p", "--port" }, () => 6910, "listen port")
            };
            root.Handler = CommandHandler.Create<RootOptions>(RootHandler);

            return root.InvokeAsync(args).Result;
        }

        private static void RootHandler(RootOptions options)
        {
            var context = new Context();
            context.Listen(options.Port);
            var randomSender = new RandomSender(context);
            Console.WriteLine($"listen port: {options.Port}");
            Console.WriteLine("Press enter to exit");
            Console.ReadLine();
        }
    }
}
