using Client.App;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLI.Receiver
{
    public enum ReceiverType
    {
        Dummy,
        Print,
    }

    public static class ReceiverFactory
    {

        public static IReceiver CreateReceiver(ReceiverType type, MediatorContext m)
        {
            var r = GenerateReceiver(type, m);
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

        private static IReceiver GenerateReceiver(ReceiverType type, MediatorContext m)
        {
            return type switch
            {
                ReceiverType.Dummy => new DummyReceiver() { MediatorContext = m },
                ReceiverType.Print => new PrintReceiver() { MediatorContext = m },
                _ => throw new ArgumentException($"Unknown receiver type: {type}"),
            };
        }
    }
}
