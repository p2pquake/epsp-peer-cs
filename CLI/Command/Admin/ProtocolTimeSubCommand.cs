using Client.App;
using Client.Client;
using Client.Client.General;
using Client.Common.General;
using Client.Peer;

using log4net.Config;

using System;
using System.Collections.Generic;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CLI.Command.Admin
{
    public class ProtocolTimeSubCommand
    {
        public static System.CommandLine.Command Build()
        {
            var command = new System.CommandLine.Command(
                "protocol-time",
                "各サーバのプロトコル日時を取得します"
                );

            command.Handler = CommandHandler.Create(ProtocolTimeHandler);
            return command;
        }
        private static void ProtocolTimeHandler()
        {
            var servers = new string[]
            {
                "www.p2pquake.net:6910",
                "p2pquake.ddo.jp:6910",
                "p2pquake.info:6910",
                "p2pquake.xyz:6910",
            };

            foreach (var server in servers)
            {
                var retreiver = new ProtocolTimeRetreiver();
                var offset = retreiver.GetOffset(server);

                if (offset == TimeSpan.MinValue)
                {
                    continue;
                }
                Console.WriteLine($"{server},{offset.TotalSeconds}");
            }
        }
    }

    class ProtocolTimeRetreiver : IPeerConfig, IPeerStateForClient, IPeerConnector
    {
        // IPeerStateForClient
        public int PeerId { get; set; }
        public TimeSpan TimeOffset { get; set; }
        public int Connections => 0;
        public bool IsPortOpened { get; set; }
        public KeyData Key { get; set; }
        public IDictionary<string, int> AreaPeerDictionary { get; set; }
        public int PeerCount => 0;

        // IPeerConfig
        public bool Verification { get => false; set { } }
        public int AreaCode { get => 900; set { } }
        public string FormattedAreaCode => "900";
        public bool IsPortOpen { get => false; set { } }
        public bool IsPortListening => false;
        public int Port { get => 6999; set { } }
        public int MaxConnections { get => 4; set { } }

        // IPeerConnector
        public event EventHandler<EventArgs> ConnectionsChanged;

        // Original
        bool connected;

        public TimeSpan GetOffset(string server)
        {
            var client = new ClientContext(new string[] { server });
            client.PeerConfig = this;
            client.PeerState = this;
            client.PeerConnector = this;
            client.OperationCompleted += Client_OperationCompleted;

            client.Join();

            var sw = new Stopwatch();
            sw.Start();
            while (sw.ElapsedMilliseconds <= 5000 && client.State.GetType().Name != "FinishedState")
            {
                Thread.Sleep(250);
            }
            sw.Stop();

            if (!connected)
            {
                return TimeSpan.MinValue;
            }

            var offset = TimeOffset;
            client.Part();

            sw.Reset();
            sw.Start();
            while (sw.ElapsedMilliseconds <= 5000 && client.State.GetType().Name != "FinishedState")
            {
                Thread.Sleep(250);
            }
            sw.Stop();

            return offset;
        }

        private void Client_OperationCompleted(object sender, OperationCompletedEventArgs e)
        {
            if (e.Result == ClientConst.OperationResult.Successful)
            {
                connected = true;
            }
        }

        // IPeerStateForClient
        public DateTime CalcNowProtocolTime()
        {
            return DateTime.Now + TimeOffset;
        }

        // IPeerConnector
        public int[] Connect(PeerData[] peers)
        {
            return Array.Empty<int>();
        }
    }
}
