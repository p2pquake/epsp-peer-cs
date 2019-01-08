using EPSPWPFClient.Controls;
using EPSPWPFClient.PeerMap;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPSPWPFClient.ViewModel
{
    public class PeerMapViewModel
    {
        public ReactiveCommand RedrawCommand { get; private set; } = new ReactiveCommand();

        public PeerMapControl PeerMapControl { private get; set; }
        public Func<IDictionary<string, int>> AreaPeerDictionary { private get; set; }

        public PeerMapViewModel()
        {
            RedrawCommand.Subscribe((i) =>
            {
                Draw();
            });
        }

        private void Draw()
        {
            Drawer.Draw(PeerMapControl.canvas, AreaPeerDictionary());
        }
    }
}
