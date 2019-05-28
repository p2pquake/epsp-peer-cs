using Client.Peer;
using EPSPWPFClient.Controls;
using EPSPWPFClient.EEWTest;
using EPSPWPFClient.Mediator;
using EPSPWPFClient.Quake;
using EPSPWPFClient.Tsunami;
using EPSPWPFClient.Userquake;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Text;
using System.Threading.Tasks;

namespace EPSPWPFClient.ViewModel
{
    public class EventViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private EPSPDataEventArgs _dataEventArgs;
        public EPSPDataEventArgs DataEventArgs
        {
            get => _dataEventArgs;
            // HACK: とりあえず動くが雑
            set { _dataEventArgs = value; DataEventArgs.PropertyChanged += (s, e) => { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Title")); }; }
        }

        public string Title { get { return EPSPTitleConverter.GetTitle(DataEventArgs); } }
    }

    public class HistoryViewModel
    {
        public ReactiveCommand RedrawCommand { get; private set; } = new ReactiveCommand();

        public ReadOnlyReactiveCollection<EventViewModel> EventList { get; private set; }
        public ReactiveProperty<int> EventIndex { get; } = new ReactiveProperty<int>();

        public Func<IDictionary<string, int>> GetAreaPeerDictionary { private get; set; }

        private EPSPHandlerFacade epspHandler;

        // HACK: MVVMの法則が乱れている感あるので後で直したい。
        public HistoryControl HistoryControl { private get; set; }
        private QuakeDrawer drawer = new QuakeDrawer();
        private TsunamiDrawer tsunamiDrawer = new TsunamiDrawer();
        private EEWDrawer eewDrawer = new EEWDrawer();

        public HistoryViewModel() : this(null, null)
        {

        }

        public HistoryViewModel(EPSPHandlerFacade epspHandler, Func<IDictionary<string, int>> getAreaPeerDictionary)
        {
            this.epspHandler = epspHandler;
            this.GetAreaPeerDictionary = getAreaPeerDictionary;

            EventIndex.Subscribe((i) =>
            {
                if (EventIndex.Value < 0 || EventList == null || EventList.Count == 0)
                {
                    return;
                }
                
                // XXX: リファクタリングできそう
                var eventArgs = EventList[EventIndex.Value].DataEventArgs;
                if (eventArgs is EPSPQuakeEventArgs)
                {
                    drawer.QuakeEventArgs = (EPSPQuakeEventArgs)eventArgs;
                    drawer.Draw(HistoryControl.canvas);
                }
                if (eventArgs is EPSPTsunamiEventArgs)
                {
                    tsunamiDrawer.EventArgs = (EPSPTsunamiEventArgs)eventArgs;
                    tsunamiDrawer.Draw(HistoryControl.canvas);
                }
                if (eventArgs is EPSPEEWTestEventArgs)
                {
                    eewDrawer.EventArgs = (EPSPEEWTestEventArgs)eventArgs;
                    eewDrawer.Draw(HistoryControl.canvas);
                }
                if (eventArgs is EPSPUQSummaryEventArgs)
                {
                    // TODO: FIXME: 情報更新時に再描画されない
                    UQDrawer.Draw(HistoryControl.canvas, ((EPSPUQSummaryEventArgs)eventArgs).Summary, GetAreaPeerDictionary());
                }
            });

            RedrawCommand.Subscribe((e) =>
            {
                if (EventIndex.Value < 0 || EventList == null || EventList.Count == 0)
                {
                    return;
                }

                var eventArgs = EventList[EventIndex.Value].DataEventArgs;
                if (eventArgs is EPSPQuakeEventArgs)
                {
                    drawer.Redraw(HistoryControl.canvas);
                }
                if (eventArgs is EPSPTsunamiEventArgs)
                {
                    tsunamiDrawer.Redraw(HistoryControl.canvas);
                }
                if (eventArgs is EPSPEEWTestEventArgs)
                {
                    eewDrawer.Redraw(HistoryControl.canvas);
                }
                if (eventArgs is EPSPUQSummaryEventArgs)
                {
                    UQDrawer.Draw(HistoryControl.canvas, ((EPSPUQSummaryEventArgs)eventArgs).Summary, GetAreaPeerDictionary());
                }
            });
        }

        public void SetScheduler(IScheduler scheduler)
        {
            EventList = epspHandler.EventList.ToReadOnlyReactiveCollection(
                epspHandler.EventList.ToCollectionChanged(),
                e => new EventViewModel() { DataEventArgs = e },
                scheduler
                );
        }
    }
}
