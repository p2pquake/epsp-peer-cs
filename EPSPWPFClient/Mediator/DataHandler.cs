using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Client.Peer;
using ClientHelper.Misc.UQSummary;
using EPSPWPFClient.Userquake;

namespace EPSPWPFClient.Mediator
{
    class DataHandler : IEPSPHandleable
    {
        public ObservableCollection<EPSPDataEventArgs> EventList { get; } = new ObservableCollection<EPSPDataEventArgs>();

        private IUQManager uqManager = new UQManager();
        private IUQJudge uqJudge = new SimpleUQJudge(3);
        private EPSPUQSummaryEventArgs headUQSummary;
        private Func<DateTime> protocolTime;

        public DataHandler(Func<DateTime> protocolTime, Func<IDictionary<string, int>> areaPeerDictionary)
        {
            this.protocolTime = protocolTime;
            uqManager.ProtocolTime = protocolTime;
            uqManager.AreaPeerDictionary = areaPeerDictionary;
            uqManager.UQJudge = uqJudge;
            uqManager.Occurred += UqManager_Occurred;
            uqManager.Updated += UqManager_Updated;

            BindingOperations.EnableCollectionSynchronization(EventList, new object());
        }

        public void OnAreapeers(EPSPAreapeersEventArgs e)
        {
            // noop
        }

        public void OnEarthquake(EPSPQuakeEventArgs e)
        {
            if (!e.IsValid) { return; }
            if (e.InformationType == QuakeInformationType.Unknown) { return; }

            InsertEventList(e);
        }

        public void OnEEWTest(EPSPEEWTestEventArgs e)
        {
            if (!e.IsValid) { return; }
            if (e.IsTest) { return; }

            InsertEventList(e);
        }

        public void OnTsunami(EPSPTsunamiEventArgs e)
        {
            if (!e.IsValid) { return; }

            InsertEventList(e);
        }

        public void OnUserquake(EPSPUserquakeEventArgs e)
        {
            if (!e.IsValid) { return; }

            uqManager.Add(e.AreaCode);
        }

        private void UqManager_Updated(object sender, EventArgs e)
        {
            headUQSummary.UpdatedAt = protocolTime();
            headUQSummary.Summary = uqManager.GetCurrentSummary();

            // FIXME: Notify EventList subscriber
        }

        private void UqManager_Occurred(object sender, EventArgs e)
        {
            headUQSummary = new EPSPUQSummaryEventArgs()
            {
                Summary = uqManager.GetCurrentSummary(),
                StartedAt = protocolTime(),
                UpdatedAt = protocolTime()
            };
            InsertEventList(headUQSummary);
        }

        private void InsertEventList(EPSPDataEventArgs e)
        {
            EventList.Insert(0, e);
            if (EventList.Count > 20)
            {
                EventList.RemoveAt(EventList.Count - 1);
            }
        }
    }
}
