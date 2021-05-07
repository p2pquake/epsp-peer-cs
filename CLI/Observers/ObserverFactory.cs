using Client.App;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLI.Observers
{
    public enum ObserverType
    {
        Dummy,
        Print,
        Grpc,
    }

    public static class ObserverFactory
    {

        public static IObserver CreateObserver(ObserverType type, MediatorContext m)
        {
            var r = GenerateObserver(type, m);
            m.StateChanged += r.StateChanged;
            m.Completed += r.Completed;
            m.ConnectionsChanged += r.ConnectionsChanged;
            m.OnEarthquake += r.OnEarthquake;
            m.OnTsunami += r.OnTsunami;
            m.OnAreapeers += r.OnAreapeers;
            m.OnEEWTest += r.OnEEWTest;
            m.OnUserquake += r.OnUserquake;
            m.OnNewUserquakeEvaluation += r.OnNewUserquakeEvaluation;
            m.OnUpdateUserquakeEvaluation += r.OnUpdateUserquakeEvaluation;

            return r;
        }

        private static IObserver GenerateObserver(ObserverType type, MediatorContext m)
        {
            return type switch
            {
                ObserverType.Dummy => new DummyObserver() { MediatorContext = m },
                ObserverType.Print => new PrintObserver() { MediatorContext = m },
                ObserverType.Grpc => GenerateGrpcObserver(m),
                _ => throw new ArgumentException($"Unknown receiver type: {type}"),
            };
        }

        // Note. 他の Observer でも似たようなことをしはじめたら IObserver に組み入れる
        private static GrpcObserver GenerateGrpcObserver(MediatorContext m)
        {
            var v = new GrpcObserver() { MediatorContext = m };
            v.Build();
            return v;
        }
    }
}
