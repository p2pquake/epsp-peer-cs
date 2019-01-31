using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using ClientHelper.Misc.UQSummary;
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

        public EPSPHandler()
        {
            BindingOperations.EnableCollectionSynchronization(EventList, new object());
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
    }
}
