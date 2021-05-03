
using Client.App;
using Client.App.Userquake;
using Client.Client;
using Client.Peer;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

using System;

namespace CLI.Observers
{
    /// <summary>
    /// gRPC サーバを建てるためのダミー Observer
    /// </summary>
    public class GrpcObserver : DummyObserver, IObserver
    {
        public void Build()
        {
            _ = new Grpc.Server(MediatorContext);
        }
    }
}
