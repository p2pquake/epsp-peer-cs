using Client.App;
using Client.App.Userquake;
using Client.Client;
using Client.Peer;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLI.Observers
{
    public class DummyObserver : IObserver
    {
        public MediatorContext MediatorContext { protected get; init; }

        public void Completed(object sender, OperationCompletedEventArgs e)
        {
        }

        public void ConnectionsChanged(object sender, EventArgs e)
        {
        }

        public void OnAreapeers(object sender, EventArgs e)
        {
        }

        public void OnEarthquake(object sender, EPSPQuakeEventArgs e)
        {
        }

        public void OnEEWTest(object sender, EPSPEEWTestEventArgs e)
        {
        }

        public void OnNewUserquakeEvaluation(object sender, UserquakeEvaluateEventArgs e)
        {
        }

        public void OnTsunami(object sender, EPSPTsunamiEventArgs e)
        {
        }

        public void OnUpdateUserquakeEvaluation(object sender, UserquakeEvaluateEventArgs e)
        {
        }

        public void OnUserquake(object sender, EPSPUserquakeEventArgs e)
        {
        }

        public void StateChanged(object sender, EventArgs e)
        {
        }
    }
}
