﻿using Client.App;
using Client.App.Userquake;
using Client.Peer;

using JsonApi;

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

using WpfClient.EPSPDataView;
using WpfClient.Notifications;

namespace WpfClient
{
    public static class Program
    {
        // FIXME: 動作するか試している。あとでリファクタリングする。
        static MediatorContext client;
        static RootViewModel viewModel;
        static Configuration configuration;
        static Notifier notifier;
        static Notifications.Activator activator;
        static Player player;

        [STAThread]
        public static void Main(string[] args)
        {
            configuration = ConfigurationManager.Configuration;

            var localMode = args.Length > 0 && args[0] == "local";
            Task.Run(() => { BootP2PQuake(localMode); });

            App app = new();
            app.SessionEnding += App_SessionEnding;
            app.InitializeComponent();
            app.Run();
        }

        public static void BootP2PQuake(bool localMode)
        {
            // ViewModel を取得したりイベントハンドラをくっつけたりする
            while (viewModel == null)
            {
                try
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        if (App.Current?.MainWindow?.DataContext != null)
                        {
                            var window = (MainWindow)App.Current?.MainWindow;
                            viewModel = (RootViewModel)window?.DataContext;
                            viewModel.SettingViewModel.LoadFromConfiguration(ConfigurationManager.Configuration);
                            window.OnExit += Window_OnExit;
                            window.OnUserquake += Window_OnUserquake;
                        }
                    });
                }
                catch
                {
                    // nothing to do
                }

                Thread.Sleep(1000);
            };

            client = new MediatorContext();
            client.ConnectionsChanged += Client_ConnectionsChanged;
            client.StateChanged += Client_StateChanged;
            client.OnAreapeers += Client_OnAreapeers;
            client.OnEarthquake += Client_OnEarthquake;
            client.OnTsunami += Client_OnTsunami;
            client.OnEEWTest += Client_OnEEWTest;
            client.OnNewUserquakeEvaluation += Client_OnNewUserquakeEvaluation;
            client.OnUpdateUserquakeEvaluation += Client_OnUpdateUserquakeEvaluation;

            configuration.OnChangeEPSPConfiguration += (s, e) =>
            {
                ReflectEPSPConfiguration();
            };
            ReflectEPSPConfiguration();

            notifier = new Notifier(configuration, client);
            activator = new Notifications.Activator(configuration, client);
            player = new Player(configuration, client);

            if (localMode)
            {
                // P2P 地震情報ネットワークに接続せず、 localhost:6910 に接続する
                var field = client.GetType().GetField("peerContext", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var peerContext = field.GetValue(client);

                var peerDataType = Type.GetType("Client.Common.General.PeerData, Client");
                var peerData = System.Activator.CreateInstance(peerDataType, new object[] { "localhost", 6910, 10 });
                var peerDatas = Array.CreateInstance(peerDataType, 1);
                peerDatas.SetValue(peerData, 0);

                var connectMethod = Type.GetType("Client.Peer.Context, Client").GetMethod("Connect");
                connectMethod.Invoke(peerContext, new object[] { peerDatas });

                return;
            }

            client.Connect();
            ReadHistories();
        }

        private static void ReflectEPSPConfiguration()
        {
            client.IsPortOpen = configuration.PortOpen;
            client.Port = configuration.Port;
            client.UseUPnP = configuration.UseUPnP;
            client.AreaCode = configuration.AreaCode;
        }

        private static void App_SessionEnding(object sender, System.Windows.SessionEndingCancelEventArgs e)
        {
            Disconnect();
        }

        private static void Window_OnExit(object sender, EventArgs e)
        {
            Disconnect();
            App.Current.Shutdown();
        }

        private static void Disconnect()
        {
            if (!client.CanDisconnect)
            {
                return;
            }

            var sw = new Stopwatch();
            sw.Start();
            client.Disconnect();

            while (sw.ElapsedMilliseconds <= 4000 && !client.CanConnect)
            {
                Thread.Sleep(250);
            }
        }

        private static void Client_OnEarthquake(object sender, EPSPQuakeEventArgs e)
        {
            AddHistory(e);
        }

        private static void Client_OnTsunami(object sender, EPSPTsunamiEventArgs e)
        {
            AddHistory(e);
        }

        private static void Client_OnEEWTest(object sender, EPSPEEWTestEventArgs e)
        {
            AddHistory(e);
        }

        private static void Client_OnNewUserquakeEvaluation(object sender, UserquakeEvaluateEventArgs e)
        {
            AddUserquakeHistory(e);
        }

        private static void Client_OnUpdateUserquakeEvaluation(object sender, UserquakeEvaluateEventArgs e)
        {
            AddUserquakeHistory(e);
        }
        
        private static void AddHistory(EventArgs e)
        {
            var obj = Factory.WrapEventArgs(e, viewModel.InformationViewModel);
            App.Current.Dispatcher.Invoke(() =>
            {
                viewModel.InformationViewModel.Histories.Insert(1, obj);
            });
        }

        private static void AddUserquakeHistory(UserquakeEvaluateEventArgs eventArgs)
        {
            var obj = Factory.WrapEventArgs(eventArgs, viewModel.InformationViewModel);
            App.Current.Dispatcher.Invoke(() =>
            {
                // 開始日時が同じものは、最新の情報だけコレクションに含める
                var histories = viewModel.InformationViewModel.Histories;
                var existItem = histories.FirstOrDefault(e => (e is EPSPUserquakeView view) && (view.EventArgs.StartedAt == eventArgs.StartedAt));

                if (existItem == null)
                {
                    histories.Insert(1, obj);
                }
                else if (existItem is EPSPUserquakeView view && view.EventArgs.UpdatedAt < eventArgs.UpdatedAt)
                {
                    var index = histories.IndexOf(existItem);
                    histories.Insert(index, obj);
                }
            });
        }

        private async static void ReadHistories()
        {
            var items = await JsonApi.Client.Get(100, Code.Earthquake, Code.Tsunami, Code.EEWTest, Code.UserquakeEvaluation);
            var histories = viewModel.InformationViewModel.Histories;

            foreach (var item in items)
            {
                if (item is JMAQuake quake)
                {
                    var eventArgs = new EPSPQuakeEventArgs()
                    {
                        InformationType = quake.Issue.Type switch
                        {
                            "ScalePrompt" => QuakeInformationType.ScalePrompt,
                            "Destination" => QuakeInformationType.Destination,
                            "ScaleAndDestination" => QuakeInformationType.ScaleAndDestination,
                            "DetailScale" => QuakeInformationType.Detail,
                            "Foreign" => QuakeInformationType.Foreign,
                            _ => QuakeInformationType.Unknown,
                        },
                        TsunamiType = quake.Earthquake.DomesticTsunami switch
                        {
                            "None" => DomesticTsunamiType.None,
                            "Checking" => DomesticTsunamiType.Checking,
                            "NonEffective" => DomesticTsunamiType.None,
                            "Watch" => DomesticTsunamiType.Effective,
                            "Warning" => DomesticTsunamiType.Effective,
                            _ => DomesticTsunamiType.Unknown,
                        },
                        Depth = quake.Earthquake.Hypocenter.Depth == 0 ? "ごく浅い" : quake.Earthquake.Hypocenter.Depth == -1 ? "不明" : $"{quake.Earthquake.Hypocenter.Depth}km",
                        Destination = quake.Earthquake.Hypocenter.Name,
                        Magnitude = $"M{quake.Earthquake.Hypocenter.Magnitude}",
                        OccuredTime = DateTime.Parse(quake.Earthquake.Time).ToString("d日HH時mm分"),
                        Scale = ConvertScale(quake.Earthquake.MaxScale),
                        Latitude = quake.Earthquake.Hypocenter.Latitude <= -200 ? "" : $"{(quake.Earthquake.Hypocenter.Latitude > 0 ? 'N' : 'S')}{Math.Abs(quake.Earthquake.Hypocenter.Latitude)}",
                        Longitude = quake.Earthquake.Hypocenter.Longitude <= -200 ? "" : $"{(quake.Earthquake.Hypocenter.Longitude > 0 ? 'E' : 'W')}{Math.Abs(quake.Earthquake.Hypocenter.Longitude)}",
                        PointList = quake.Points.Select(e =>
                            new QuakeObservationPoint() { Prefecture = e.Pref, Name = e.Addr, Scale = ConvertScale(e.Scale) }
                        ).ToList(),
                    };

                    if (eventArgs.InformationType == QuakeInformationType.Unknown) { continue; }
                    AddHistory(eventArgs);
                }

                if (item is JMATsunami tsunami)
                {
                    var eventArgs = new EPSPTsunamiEventArgs()
                    {
                        IsCancelled = tsunami.Cancelled,
                        ReceivedAt = DateTime.Parse(tsunami.Time),
                        RegionList = tsunami.Areas.Select(e => new TsunamiForecastRegion()
                        {
                            IsImmediately = e.Immediate,
                            Category = e.Grade switch {
                                "MajorWarning" => TsunamiCategory.MajorWarning,
                                "Warning" => TsunamiCategory.Warning,
                                "Watch" => TsunamiCategory.Advisory,
                                _ => TsunamiCategory.Unknown,
                            },
                            Region = e.Name
                        }).ToList(),
                    };
                    AddHistory(eventArgs);
                }

                if (item is EEWDetection eew)
                {

                    var eventArgs = new EPSPEEWTestEventArgs()
                    {
                        IsTest = !(eew.Type == "Full"),
                        ReceivedAt = DateTime.Parse(eew.Time),
                    };
                    if (eventArgs.IsTest) { continue; }
                    AddHistory(eventArgs);
                }

                if (item is UserquakeEvaluation evaluation)
                {
                    if (evaluation.Confidence <= 0) { continue; }

                    var eventArgs = new UserquakeEvaluateEventArgs()
                    {
                        // XXX: JSON deserializer 側で DateTime にしておいてほしいかも
                        StartedAt = DateTime.Parse(evaluation.StartedAt),
                        UpdatedAt = DateTime.Parse(evaluation.UpdatedAt),
                        Count = evaluation.Count,
                        Confidence = evaluation.Confidence,
                        AreaConfidences = evaluation.AreaConfidences.Where(e => e.Value.Confidence >= 0).ToDictionary(e => e.Key, e => new UserquakeEvaluationArea() { AreaCode = e.Key, Confidence = e.Value.Confidence, Count = e.Value.Count } as IUserquakeEvaluationArea)
                    };

                    AddUserquakeHistory(eventArgs);
                }
            }
        }

        private static string ConvertScale(int scale)
        {
            return scale switch
            {
                10 => "1",
                20 => "2",
                30 => "3",
                40 => "4",
                45 => "5弱",
                50 => "5強",
                55 => "6弱",
                60 => "6強",
                70 => "7",
                _ => "不明",
            };
        }

        private static void Window_OnUserquake(object sender, EventArgs e)
        {
            client.SendUserquake();
        }

        private static void Client_ConnectionsChanged(object sender, EventArgs e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                viewModel.StatusViewModel.Connections = $"{client.Connections} / {client.MaxConnections} ピア";
            });
        }

        private static void Client_OnAreapeers(object sender, EventArgs e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                viewModel.NumberOfPeersLabel = $"{client.PeerCount} ピア";
                viewModel.StatusViewModel.NumberOfPeers = $"{client.PeerCount} ピア";

                viewModel.StatusViewModel.AreapeerText = String.Join("、 ", client.AreaPeerDictionary.Select((area) => $"{area.Key}: {area.Value}ピア"));
            });
        }

        private static void Client_StateChanged(object sender, EventArgs e)
        {
            // 未接続、切断中 = 切断アイコン
            var status = ChoiceByState(
                disconnected: "未接続",
                disconnecting: "切断中...",
                connecting: "接続中...",
                connected: "接続済み",
                noConnection: "接続なし"
                );

            var statusIconGlyph = ChoiceByState(
                disconnected: "\xF384",
                disconnecting: "\xF384",
                connecting: "\xF384",
                connected: "\xEC27",
                noConnection: "\xF384"
                );

            var statusTitle = ChoiceByState(
                disconnected: "サーバーに接続できません。",
                disconnecting: "切断しています...",
                connecting: "接続を開始しています...",
                connected: "正常に接続されています。",
                noConnection: "ピアと接続されていません。"
                );

            var statusDescription = ChoiceByState(
                disconnected: "サーバーに接続できないため、P2P地震情報のネットワークに接続できませんでした。自動的にリトライします。",
                disconnecting: "P2P地震情報のネットワークから切断しようとしています。",
                connecting: "P2P地震情報のネットワークへ接続しようとしています。",
                connected: "P2P地震情報のネットワークに接続されており、最新の情報をリアルタイムに受信できる状態です。",
                noConnection: "ピアと接続されていないため、最新の情報が受信できません。自動的にリトライします。"
                );

            var portStatus = "-";
            if (client.State.GetType().Name == "ConnectedState")
            {
                portStatus = client.IsPortOpened ? "開放" : "未開放";
            }

            var hasValidKey =
                client.Key != null && !client.Key.IsExpired(client.CalcNowProtocolTime());

            App.Current.Dispatcher.Invoke(() =>
            {
                viewModel.Status = status;
                viewModel.StatusIconGlyph = statusIconGlyph;
                viewModel.PortStatus = $"ポート: {portStatus}";
                viewModel.CanSendUserquake = hasValidKey;

                viewModel.StatusViewModel.PortStatus = portStatus;
                viewModel.StatusViewModel.KeyStatus = hasValidKey ? "有効" : "無効";
                viewModel.StatusViewModel.StatusTitle = statusTitle;
                viewModel.StatusViewModel.StatusDescription = statusDescription;
            });
        }

        private static string ChoiceByState(string disconnected, string disconnecting, string connecting, string connected, string noConnection)
        {
            return client.State.GetType().Name switch
            {
                "DisconnectedState" => disconnected,
                "DisconnectingState" => disconnecting,
                "ConnectingState" => connecting,
                _ => client.Connections > 0 ? connected : noConnection
            };
        }
    }
}
