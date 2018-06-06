using Client.Peer;
using EPSPWPFClient.Mediator;
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
        public ReadOnlyReactiveCollection<EventViewModel> EventList { get; private set; }
        public ReactiveProperty<int> EventIndex { get; } = new ReactiveProperty<int>();
        private EPSPHandler epspHandler;

        public HistoryViewModel(EPSPHandler epspHandler)
        {
            this.epspHandler = epspHandler;

            EventIndex.Subscribe(e =>
            {
                Console.WriteLine(EventIndex.Value);
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
