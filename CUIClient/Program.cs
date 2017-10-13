using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.App;

namespace CUIClient
{
    class Program
    {
        private static IMediatorContext mediatorContext;

        static void Main(string[] args)
        {
            mediatorContext = new MediatorContext();
            mediatorContext.ConnectionsChanged += MediatorContext_ConnectionsChanged;
            mediatorContext.OnAreapeers += MediatorContext_OnAreapeers;
            mediatorContext.OnEarthquake += MediatorContext_OnEarthquake;
            mediatorContext.OnTsunami += MediatorContext_OnTsunami;
            mediatorContext.OnUserquake += MediatorContext_OnUserquake;
            mediatorContext.StateChanged += MediatorContext_StateChanged;

            mediatorContext.Connect();

            Console.ReadLine();
        }

        private static void MediatorContext_StateChanged(object sender, EventArgs e)
        {
            Console.WriteLine("MediatorContext changed to " + mediatorContext.State.ToString());
            // throw new NotImplementedException();
        }

        private static void MediatorContext_OnUserquake(object sender, Client.Peer.EPSPUserquakeEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void MediatorContext_OnTsunami(object sender, Client.Peer.EPSPTsunamiEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void MediatorContext_OnEarthquake(object sender, Client.Peer.EPSPQuakeEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void MediatorContext_OnAreapeers(object sender, Client.Peer.EPSPAreapeersEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void MediatorContext_ConnectionsChanged(object sender, EventArgs e)
        {
            Console.WriteLine("Connection: " + mediatorContext.Connections);
        }
    }
}
