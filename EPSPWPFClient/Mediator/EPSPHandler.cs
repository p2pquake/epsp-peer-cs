using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Client.Misc.UQSummary;
using Client.Peer;
using log4net;

namespace EPSPWPFClient.Mediator
{
    /// <summary>
    /// EPSPの情報を処理するサンプルクラスです。
    /// </summary>
    public class EPSPHandler
    {
        public ObservableCollection<EPSPDataEventArgs> EventList { get; } = new ObservableCollection<EPSPDataEventArgs>();
        private IUQManager uqManager = new UQManager();

        public EPSPHandler(Func<DateTime> protocolTimeFunc, Func<IDictionary<string, int>> areaPeerDictionaryFunc)
        {
            BindingOperations.EnableCollectionSynchronization(EventList, new object());

            uqManager.UQJudge = new SimpleUQJudge(3);
            uqManager.ProtocolTime = protocolTimeFunc;
            uqManager.AreaPeerDictionary = areaPeerDictionaryFunc;
            uqManager.Occurred += UqManager_Occurred;
            uqManager.Updated += UqManager_Updated;
        }

        private void UqManager_Updated(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void UqManager_Occurred(object sender, EventArgs e)
        {
            var uqSummaryEventArgs = new EPSPUQSummaryEventArgs()
            {
                Summary = uqManager.GetCurrentSummary(),
                IsExpired = false,
                IsInvalidSignature = false
            };
            EventList.Add(uqSummaryEventArgs);
        }

        /// <summary>
        /// 地震情報のイベント処理
        /// </summary>
        public void MediatorContext_OnEarthquake(object sender, EPSPQuakeEventArgs e)
        {
            if (!e.IsValid) { return; }
            if (e.InformationType == QuakeInformationType.Unknown) { return; }

            EventList.Add(e);
        }

        /// <summary>
        /// 津波予報のイベント処理
        /// </summary>
        public void MediatorContext_OnTsunami(object sender, EPSPTsunamiEventArgs e)
        {
            if (!e.IsValid) { return; }

            EventList.Add(e);
        }

        /// <summary>
        /// 緊急地震速報 配信試験のイベント処理
        /// </summary>
        internal void MediatorContext_OnEEWTest(object sender, EPSPEEWTestEventArgs e)
        {
            if (!e.IsValid) { return; }
            if (e.IsTest) { return; }

            EventList.Add(e);
        }

        /// <summary>
        /// 地震感知情報のイベント処理
        /// </summary>
        internal void MediatorContext_OnUserquake(object sender, EPSPUserquakeEventArgs e)
        {
            if (!e.IsValid) { return; }

            uqManager.Add(e.AreaCode);
        }
    }
}
