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
            EventList = dataHandler.EventList;
            handleables.Add(dataHandler);

            handleables.Add(new NotifyHandler());
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
