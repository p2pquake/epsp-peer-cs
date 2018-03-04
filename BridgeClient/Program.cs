using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.App;
using log4net;
using Newtonsoft.Json;
using StatePrinting;
using StatePrinting.OutputFormatters;

namespace BridgeClient
{
#if RAISE_RAW_DATA_EVENT || MOBILE_SERVER
    class Program
    {
        private static ILog logger = LogManager.GetLogger("BridgeClient");
        private static ILog dataLogger = LogManager.GetLogger("DataLogger");

        private static IMediatorContext mediatorContext;
        private static MobileServer mobileServer;
        private static Stateprinter statePrinter = new Stateprinter();

        static void Main(string[] args)
        {
            logger.Info("boot.");

            statePrinter.Configuration.SetOutputFormatter(new JsonStyle(statePrinter.Configuration));

            mediatorContext = new MediatorContext();
            mediatorContext.ConnectionsChanged += MediatorContext_ConnectionsChanged;
            mediatorContext.OnAreapeers += MediatorContext_OnAreapeers;
            mediatorContext.OnEarthquake += MediatorContext_OnEarthquake;
            mediatorContext.OnTsunami += MediatorContext_OnTsunami;
            mediatorContext.OnEEWTest += MediatorContext_OnEEWTest;
            mediatorContext.OnUserquake += MediatorContext_OnUserquake;
            mediatorContext.OnData += MediatorContext_OnData;
            mediatorContext.StateChanged += MediatorContext_StateChanged;
            mediatorContext.Completed += MediatorContext_Completed;

            mediatorContext.IsPortOpen = true;
            mediatorContext.Port = 6999;

            mobileServer = new MobileServer();
            mobileServer.OnReceive += MobileServer_OnReceive;
            mobileServer.Start();

            mediatorContext.Connect();

            Console.ReadLine();
        }

        private static void MobileServer_OnReceive(object sender, ReceiveEventArgs e)
        {
            logger.Debug("arrival from mobileserver: " + statePrinter.PrintObject(e));
            mediatorContext.SendAll(e.Packet);
        }

        private static void MediatorContext_OnData(object sender, Client.Peer.EPSPRawDataEventArgs e)
        {
            dataLogger.Info("Data: " + e.Packet);
        }

        private static void MediatorContext_Completed(object sender, Client.Client.OperationCompletedEventArgs e)
        {
            logger.Debug("completed: " + statePrinter.PrintObject(e));
        }

        private static void MediatorContext_StateChanged(object sender, EventArgs e)
        {
            logger.Debug("state-changed: " + statePrinter.PrintObject(e));
        }

        private static void MediatorContext_OnUserquake(object sender, Client.Peer.EPSPUserquakeEventArgs e)
        {
            dataLogger.Info("Userquake: " + JsonConvert.SerializeObject(e));
        }

        private static void MediatorContext_OnTsunami(object sender, Client.Peer.EPSPTsunamiEventArgs e)
        {
            dataLogger.Info("Tsunami: " + JsonConvert.SerializeObject(e));
        }

        private static void MediatorContext_OnEarthquake(object sender, Client.Peer.EPSPQuakeEventArgs e)
        {
            dataLogger.Info("Earthquake: " + JsonConvert.SerializeObject(e));
        }

        private static void MediatorContext_OnAreapeers(object sender, Client.Peer.EPSPAreapeersEventArgs e)
        {
            dataLogger.Info("Areapeers: " + JsonConvert.SerializeObject(e));
        }

        private static void MediatorContext_OnEEWTest(object sender, Client.Peer.EPSPEEWTestEventArgs e)
        {
            dataLogger.Info("EEWTest: " + JsonConvert.SerializeObject(e));
        }

        private static void MediatorContext_ConnectionsChanged(object sender, EventArgs e)
        {
            logger.Debug("Connection: " + mediatorContext.Connections);
        }
    }
#endif
}
