using System;
using System.CommandLine;
using System.CommandLine.Invocation;

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
            Console.WriteLine($"port: {options.Port}");
            Console.WriteLine("Hello world");
        }
    }
}
