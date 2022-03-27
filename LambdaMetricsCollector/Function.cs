using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;
using Amazon.Lambda.Core;

using Client.App;
using Client.Client;
using Client.Client.General;
using Client.Common.General;
using Client.Peer;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace LambdaMetricsCollector
{
    public class Function
    {
        
        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public string FunctionHandler(object input, ILambdaContext context)
        {
            var servers = new string[]
            {
                "www.p2pquake.net:6910",
                "p2pquake.ddo.jp:6910",
                "p2pquake.info:6910",
                "p2pquake.xyz:6910",
            };

            var request = new PutMetricDataRequest();
            request.MetricData = new();
            request.Namespace = "epsp";

            foreach (var server in servers)
            {
                var retreiver = new ProtocolTimeRetreiver();
                var offset = retreiver.GetOffset(server);
                if (offset == TimeSpan.MinValue)
                {
                    continue;
                }

                // JST Ç∆ç∑Ç™Ç†ÇÈèÍçáÇÕï‚ê≥Çì¸ÇÍÇÈ
                var jstZone = TimeZoneInfo.CreateCustomTimeZone("jst", TimeSpan.FromMinutes(60 * 9), null, null);
                var localZone = TimeZoneInfo.Local;

                if (!jstZone.HasSameRules(localZone))
                {
                    offset += (localZone.BaseUtcOffset - jstZone.BaseUtcOffset);
                }

                var peerDict = retreiver.AreaPeerDictionary;
                var peers = peerDict.Values.Sum();

                request.MetricData.Add(new MetricDatum()
                {
                    MetricName = "TimeOffset",
                    Unit = StandardUnit.Seconds,
                    Value = offset.TotalSeconds,
                    Dimensions = new List<Dimension>
                    {
                        new Dimension() { Name = "ServerName", Value = server.Split(':')[0] },
                    },
                });
                request.MetricData.Add(new MetricDatum()
                {
                    MetricName = "NumberOfPeers",
                    Unit = StandardUnit.Count,
                    Value = peers,
                    Dimensions = new List<Dimension>
                    {
                        new Dimension() { Name = "ServerName", Value = server.Split(':')[0] },
                    },
                });
            }

            var client = new AmazonCloudWatchClient(Amazon.RegionEndpoint.APNortheast1);
            client.PutMetricDataAsync(request).Wait();

            return "ok";
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
