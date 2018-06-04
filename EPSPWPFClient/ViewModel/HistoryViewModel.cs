using Client.Peer;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPSPWPFClient.ViewModel
{
    public class History
    {
        public string Title { get; set; }
        public EPSPDataEventArgs DataEventArgs { get; set; }
    }

    public class HistoryViewModel
    {
        public ReactiveProperty<List<History>> HistoryList { get; } = new ReactiveProperty<List<History>>(new List<History>());
        public ReactiveProperty<int> HistoryIndex { get; } = new ReactiveProperty<int>();

        public HistoryViewModel(Mediator.EPSPHandler epspHandler)
        {
            epspHandler.EventList.ToReadOnlyReactiveCollection(epspHandler.EventList.ToCollectionChanged());

            HistoryIndex.Subscribe(e =>
            {
                Console.WriteLine(HistoryIndex.Value);
            });
        }
    }
}
