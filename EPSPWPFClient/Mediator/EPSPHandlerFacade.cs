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
using EPSPWPFClient.Userquake;

namespace EPSPWPFClient.Mediator
{
    /// <summary>
    /// EPSPの情報を処理するサンプルクラスです。
    /// </summary>
    public class EPSPHandlerFacade
    {
        private List<IEPSPHandleable> handleables;
        public ObservableCollection<EPSPDataEventArgs> EventList { get; private set; }

        public EPSPHandlerFacade(Func<DateTime> protocolTime, Func<IDictionary<string, int>> areaPeerDictionary)
        {
            handleables = new List<IEPSPHandleable>();

            var dataHandler = new DataHandler(protocolTime, areaPeerDictionary);
            dataHandler.UserquakeOccured += DataHandler_UserquakeOccured;
            dataHandler.UserquakeUpdated += DataHandler_UserquakeUpdated;
            EventList = dataHandler.EventList;

            handleables.Add(dataHandler);
            handleables.Add(new NotifyHandler());
            handleables.Add(new ShowHandler());
            handleables.Add(new RedrawHandler());
        }

        /// <summary>
        /// 地震感知情報 表示しきい値を満たした場合
        /// </summary>
        private void DataHandler_UserquakeOccured(object sender, EPSPUQSummaryEventArgs e)
        {
            handleables.ForEach(h => h.OnUserquakeReached(e));
        }

        /// <summary>
        /// 地震感知情報 表示しきい値を満たし更新された場合
        /// </summary>
        private void DataHandler_UserquakeUpdated(object sender, EPSPUQSummaryEventArgs e)
        {
            handleables.ForEach(h => h.OnUserquakeUpdated(e));
        }

        /// <summary>
        /// 地震情報のイベント処理
        /// </summary>
        public void MediatorContext_OnEarthquake(object sender, EPSPQuakeEventArgs e)
        {
            handleables.ForEach(h => h.OnEarthquake(e));
        }

        /// <summary>
        /// 津波予報のイベント処理
        /// </summary>
        public void MediatorContext_OnTsunami(object sender, EPSPTsunamiEventArgs e)
        {
            handleables.ForEach(h => h.OnTsunami(e));
        }

        /// <summary>
        /// 緊急地震速報 配信試験のイベント処理
        /// </summary>
        internal void MediatorContext_OnEEWTest(object sender, EPSPEEWTestEventArgs e)
        {
            handleables.ForEach(h => h.OnEEWTest(e));
        }

        /// <summary>
        /// 地震感知情報のイベント処理
        /// </summary>
        internal void MediatorContext_OnUserquake(object sender, EPSPUserquakeEventArgs e)
        {
            handleables.ForEach(h => h.OnUserquake(e));
        }
    }
}
