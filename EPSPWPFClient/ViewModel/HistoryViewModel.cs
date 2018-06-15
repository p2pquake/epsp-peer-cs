using Client.Peer;
using EPSPWPFClient.Controls;
using EPSPWPFClient.Mediator;
using EPSPWPFClient.Quake;
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
        public string Title { get { return DataEventArgs.GetType().Name; } }
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

                if (EventList[EventIndex.Value].DataEventArgs is EPSPQuakeEventArgs)
                {
                    drawer.QuakeEventArgs = (EPSPQuakeEventArgs)EventList[EventIndex.Value].DataEventArgs;
                    drawer.Draw(HistoryControl.canvas);
                }
            });

            RedrawCommand.Subscribe((e) =>
            {
                drawer.Redraw(HistoryControl.canvas);
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
