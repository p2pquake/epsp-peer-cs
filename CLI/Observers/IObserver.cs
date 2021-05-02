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
    public interface IObserver
    {
        MediatorContext MediatorContext { init; }

        void StateChanged(object sender, EventArgs e);
        void Completed(object sender, OperationCompletedEventArgs e);
        void ConnectionsChanged(object sender, EventArgs e);
        void OnEarthquake(object sender, EPSPQuakeEventArgs e);
        void OnTsunami(object sender, EPSPTsunamiEventArgs e);
        void OnAreapeers(object sender, EventArgs e);
        void OnEEWTest(object sender, EPSPEEWTestEventArgs e);
        void OnUserquake(object sender, EPSPUserquakeEventArgs e);
        void OnNewUserquakeEvaluation(object sender, UserquakeEvaluateEventArgs e);
        void OnUpdateUserquakeEvaluation(object sender, UserquakeEvaluateEventArgs e);
    }
}
