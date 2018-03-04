using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.App;
using StatePrinting;
using StatePrinting.OutputFormatters;

namespace CUIClient
{
    class Program
    {
        private static IMediatorContext mediatorContext;
        private static Stateprinter statePrinter = new Stateprinter();

        static void Main(string[] args)
        {
            statePrinter.Configuration.SetOutputFormatter(new JsonStyle(statePrinter.Configuration));

            mediatorContext = new MediatorContext();
            mediatorContext.ConnectionsChanged += MediatorContext_ConnectionsChanged;
            mediatorContext.OnAreapeers += MediatorContext_OnAreapeers;
            mediatorContext.OnEarthquake += MediatorContext_OnEarthquake;
            mediatorContext.OnTsunami += MediatorContext_OnTsunami;
            mediatorContext.OnEEWTest += MediatorContext_OnEEWTest;
            mediatorContext.OnUserquake += MediatorContext_OnUserquake;
            mediatorContext.StateChanged += MediatorContext_StateChanged;
            mediatorContext.Completed += MediatorContext_Completed;

            mediatorContext.IsPortOpen = true;
            mediatorContext.Port = 6999;

            mediatorContext.Connect();

            Console.ReadLine();
        }

        private static void MediatorContext_Completed(object sender, Client.Client.OperationCompletedEventArgs e)
        {
            Console.WriteLine("MediatorContext completed");
            Console.WriteLine(statePrinter.PrintObject(e));
        }

        private static void MediatorContext_StateChanged(object sender, EventArgs e)
        {
            Console.WriteLine("MediatorContext changed to " + mediatorContext.State.ToString());
            Console.WriteLine(statePrinter.PrintObject(e));
            // throw new NotImplementedException();
        }

        private static void MediatorContext_OnUserquake(object sender, Client.Peer.EPSPUserquakeEventArgs e)
        {
            // FIXME: 実装する
            Console.WriteLine("OnUserquake");
            Console.WriteLine(statePrinter.PrintObject(e));
        }

        private static void MediatorContext_OnTsunami(object sender, Client.Peer.EPSPTsunamiEventArgs e)
        {
            // FIXME: 実装する
            Console.WriteLine("OnTsunami");
            Console.WriteLine(statePrinter.PrintObject(e));
        }

        private static void MediatorContext_OnEarthquake(object sender, Client.Peer.EPSPQuakeEventArgs e)
        {
            // FIXME: 実装する
            Console.WriteLine("OnEarthquake");
            Console.WriteLine(statePrinter.PrintObject(e));
        }

        private static void MediatorContext_OnAreapeers(object sender, Client.Peer.EPSPAreapeersEventArgs e)
        {
            // FIXME: 実装する
            Console.WriteLine("OnAreapeers");
            Console.WriteLine(statePrinter.PrintObject(e));
        }

        private static void MediatorContext_OnEEWTest(object sender, Client.Peer.EPSPEEWTestEventArgs e)
        {
            // FIXME: 実装する
            Console.WriteLine("OnEEWTest");
            Console.WriteLine(statePrinter.PrintObject(e));
        }

        private static void MediatorContext_ConnectionsChanged(object sender, EventArgs e)
        {
            Console.WriteLine("Connection: " + mediatorContext.Connections);
        }
    }
}
