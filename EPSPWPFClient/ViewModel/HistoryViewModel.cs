using Client.Peer;
using EPSPWPFClient.Controls;
using EPSPWPFClient.EEWTest;
using EPSPWPFClient.Mediator;
using EPSPWPFClient.Quake;
using EPSPWPFClient.Tsunami;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Text;
using System.Threading.Tasks;

namespace EPSPWPFClient.ViewModel
{
    public class EventViewModel
    {
        public string Title { get { return EPSPTitleConverter.GetTitle(DataEventArgs); } }
        public EPSPDataEventArgs DataEventArgs { get; set; }
    }

    public class HistoryViewModel
    {
        public ReactiveCommand RedrawCommand { get; private set; } = new ReactiveCommand();

        public ReadOnlyReactiveCollection<EventViewModel> EventList { get; private set; }
        public ReactiveProperty<int> EventIndex { get; } = new ReactiveProperty<int>();

        private EPSPHandler epspHandler;

        // FIXME: MVVMの法則が乱れている感あるので後で直したい。
        public HistoryControl HistoryControl { private get; set; }
        private QuakeDrawer drawer = new QuakeDrawer();
        private TsunamiDrawer tsunamiDrawer = new TsunamiDrawer();
        private EEWDrawer eewDrawer = new EEWDrawer();

        public HistoryViewModel() : this(null)
        {

        }

        public HistoryViewModel(EPSPHandler epspHandler)
        {
            this.epspHandler = epspHandler;

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
