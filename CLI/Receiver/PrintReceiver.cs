using Client.App;
using Client.App.Userquake;
using Client.Client;
using Client.Peer;

using log4net;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLI.Receiver
{
    class PrintReceiver : IReceiver
    {
        private readonly ILog log = LogManager.GetLogger("PrintReceiver");
        public MediatorContext MediatorContext { private get; init; }

        public void Completed(object sender, OperationCompletedEventArgs e)
        {
            log.Info($"Completed. s: {sender}, e: {ObjectDumper.Dump(e)}");
        }

        public void ConnectionsChanged(object sender, EventArgs e)
        {
            log.Info($"ConnectionsChanged. s: {sender}, e: {ObjectDumper.Dump(e)}");
        }

        public void OnAreapeers(object sender, EventArgs e)
        {
            log.Info($"OnAreapeers. s: {sender}, e: {ObjectDumper.Dump(e)}");
        }

        public void OnEarthquake(object sender, EPSPQuakeEventArgs e)
        {
            log.Info($"OnEarthquake. s: {sender}, e: {ObjectDumper.Dump(e)}");
        }

        public void OnEEWTest(object sender, EPSPEEWTestEventArgs e)
        {
            log.Info($"OnEEWTest. s: {sender}, e: {ObjectDumper.Dump(e)}");
        }

        public void OnNewUserquakeEvaluation(object sender, UserquakeEvaluateEventArgs e)
        {
            log.Info($"OnNewUserquakeEvaluation. s: {sender}, e: {ObjectDumper.Dump(e)}");
        }

        public void OnTsunami(object sender, EPSPTsunamiEventArgs e)
        {
            log.Info($"OnTsunami. s: {sender}, e: {ObjectDumper.Dump(e)}");
        }

        public void OnUpdateUserquakeEvaluation(object sender, UserquakeEvaluateEventArgs e)
        {
            log.Info($"OnUpdateUserquakeEvaluation. s: {sender}, e: {ObjectDumper.Dump(e)}");
        }

        public void OnUserquake(object sender, EPSPUserquakeEventArgs e)
        {
            log.Info($"OnUserquake. s: {sender}, e: {ObjectDumper.Dump(e)}");
        }

        public void StateChanged(object sender, EventArgs e)
        {
            log.Info($"StateChanged. s: {sender}, e: {ObjectDumper.Dump(e)}");
        }
    }
}
