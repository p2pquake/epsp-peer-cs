
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace CLI.Observers
{
    class GrpcObserver : DummyObserver, IObserver
    {
        public GrpcObserver()
        {
            // Additional configuration is required to successfully run gRPC on macOS.
            // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
            Host.CreateDefaultBuilder(null)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                }).Build().RunAsync();
        }
    }
}
