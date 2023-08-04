using Updater;

using Client.App;
using Client.App.Userquake;
using Client.Peer;

using JsonApi;

using Map.Controller;
using Map.Model;

using Sentry;

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

using WpfClient.EPSPDataView;
using WpfClient.Notifications;
using WpfClient.Utils;

using TsunamiCategory = Client.Peer.TsunamiCategory;
using System.Net.Sockets;

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
        static DateTime latestConnected = DateTime.MaxValue;
        static DateTime latestUpdateCheck = DateTime.MinValue;

        private const int HistoryLimit = 100;

        [STAThread]
        public static void Main(string[] args)
        {
            configuration = ConfigurationManager.Configuration;

            var localMode = args.Length > 0 && args[0] == "local";
            var noautoupdate = args.Length > 0 && args[0] == "noautoupdate";

            using var mutex = new Mutex(false, "P2PQuake");
            bool hasHandle = false;
            try
            {
                hasHandle = mutex.WaitOne(0, false);
            }
            catch (AbandonedMutexException)
            {
                hasHandle = true;
            }
            if (!hasHandle)
            {
                // 既に起動しているプロセスをアクティブにする
                using var pipe = new NamedPipeClientStream(".", IPC.Const.Name, PipeDirection.Out, PipeOptions.CurrentUserOnly);
                try
                {
                    pipe.Connect(500);
                    using var stream = new StreamWriter(pipe);
                    stream.AutoFlush = true;
                    stream.WriteLine(JsonSerializer.Serialize(new IPC.Message(IPC.Method.Show)));
                }
                catch (TimeoutException)
                {
                    MessageBox.Show("既に起動しています。", "P2P地震情報");
                }
                return;
            }
            Task.Run(() => { RunNamedPipe(); });

            if (Updater.HasUpdaterUpdate())
            {
                Task.Run(() => { Updater.UpdateUpdater(); });
            } else if (configuration.AutoUpdate && !noautoupdate)
            {
                Updater.Run();
            }

            Task.Run(() => { BootP2PQuake(localMode); });

            InitSentry();

            App app = new();
            app.InitializeComponent();
            app.SessionEnding += App_SessionEnding;
            app.DispatcherUnhandledException += App_DispatcherUnhandledException;
            app.Run();
        }

        private static void InitSentry()
        {
            SentrySdk.Init(o =>
            {
                o.Dsn = "https://b78379893a4d4b6bb0d49fe7ae5c114b@o1151228.ingest.sentry.io/6227705";
#if DEBUG
                o.Environment = "debug";
#else
                o.Environment = "release";
#endif
                o.BeforeSend = (sentryEvent) =>
                {
                    // SocketException の OperationAborted は除外
                    if (sentryEvent.Exception is SocketException && (sentryEvent.Exception as SocketException).SocketErrorCode == SocketError.OperationAborted)
                    {
                        return null;
                    }

                    if (sentryEvent.Exception is AggregateException)
                    {
                        var inners = (sentryEvent.Exception as AggregateException).InnerExceptions;
                        if (inners.All((inner) => (inner is SocketException) && (inner as SocketException).SocketErrorCode == SocketError.OperationAborted))
                        {
                            return null;
                        }
                    }
                    return sentryEvent;
                };
            });
        }

        private static void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            SentrySdk.CaptureException(e.Exception);
        }

        private static void RunNamedPipe()
        {
            while (true)
            {
                using var pipe = new NamedPipeServerStream(IPC.Const.Name, PipeDirection.In);

                pipe.WaitForConnection();

                using var sr = new StreamReader(pipe);
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    var message = JsonSerializer.Deserialize<IPC.Message>(line);

                    switch (message.Method)
                    {
                        case IPC.Method.Show:
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                var window = App.Current.MainWindow;
                                window.Show();
                                if (window.WindowState == WindowState.Minimized)
                                {
                                    window.WindowState = WindowState.Normal;
                                }
                                window.Activate();
                            });
                            break;
                        case IPC.Method.Exit:
                            Disconnect();
                            HideNotifyIcon();
                            if (App.Current != null)
                            {
                                App.Current.Dispatcher.Invoke(() => App.Current?.Shutdown());
                            } else
                            {
                                Environment.Exit(0);
                            }
                            return;
                        default:
                            Console.Error.WriteLine($"不明なコマンド {message.Method}");
                            return;
                    }
                }
            }
        }

        private static void BootP2PQuake(bool localMode)
        {
            client = new MediatorContext();

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

                            // 設定反映
                            viewModel.SettingViewModel.LoadFromConfiguration(ConfigurationManager.Configuration);
                            if (ConfigurationManager.IsFirstBoot) {
                                viewModel.InformationViewModel.TutorialVisibility = Visibility.Visible;
                                ConfigurationManager.Save();
                            }
                            viewModel.SettingViewModel.MediatorContext = client;
                            window.OnExit += Window_OnExit;
                            window.OnUserquake += Window_OnUserquake;
                        }
                    });
                }
                catch
                {
                    // nothing to do
                }

                Thread.Sleep(250);
            };

            client.ConnectionsChanged += Client_ConnectionsChanged;
            client.StateChanged += Client_StateChanged;
            client.OnAreapeers += Client_OnAreapeers;
            client.OnEarthquake += Client_OnEarthquake;
            client.OnTsunami += Client_OnTsunami;
            client.OnEEWTest += Client_OnEEWTest;
            client.OnEEW += Client_OnEEW;
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
                // 検証鍵をテスト用のものに差し替える
                var verifierType = Type.GetType("PKCSPeerCrypto.Verifier, PKCSPeerCrypto");
                var serverProofKey = verifierType.GetField("ServerProofKey", BindingFlags.NonPublic | BindingFlags.Static);
                serverProofKey.SetValue(null, "MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDB+t0YTWlu3FFiwTb05u2bHWJRpCQJeAzhia6pWJ5BqsVIXgG7zeiHu4cFWrKME7fHQsjlihjnhtaksEtkmnbODUHnNi26FStSSpyo8ex0FZDfXtoQ9VB0m6UxdrGznpzfO9PWbpC0iSoCAyeqILLcDDbuBv5xY6+0D35kQx74kQIDAQAB");

                client.Verification = false;

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

        public static void RefreshInformation()
        {
            App.Current?.Dispatcher?.Invoke(() =>
            {
                viewModel.InformationViewModel.Histories.Clear();
            });
            ReadHistories();
        }

        private static void ReflectEPSPConfiguration()
        {
            client.IsPortOpen = configuration.PortOpen;
            client.Port = configuration.Port;
            client.MaxConnections = configuration.PortOpen ? 20 : 4;
            client.UseUPnP = configuration.UseUPnP;
            client.AreaCode = configuration.AreaCode;
        }

        private static void App_SessionEnding(object sender, System.Windows.SessionEndingCancelEventArgs e)
        {
            Disconnect();
            HideNotifyIcon();
        }

        private static void Window_OnExit(object sender, EventArgs e)
        {
            Disconnect();
            HideNotifyIcon();
            App.Current.Shutdown();
        }

        private static void Disconnect()
        {
            if (client == null) { return; }

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

        private static void HideNotifyIcon()
        {
            App.Current?.Dispatcher?.Invoke(() =>
            {
                var window = (MainWindow)App.Current?.MainWindow;
                window.HideNotifyIcon();
            });
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
            // Deprecated: 緊急地震速報（警報）に置き換えます
            //AddHistory(e);
        }

        private static void Client_OnEEW(object sender, EPSPEEWEventArgs e)
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
            var obj = Factory.WrapEventArgs(e, client.CalcNowProtocolTime, viewModel.InformationViewModel);
            App.Current.Dispatcher.Invoke(() =>
            {
                viewModel.InformationViewModel.Histories.Insert(0, obj);
                if (viewModel.InformationViewModel.Histories.Count > HistoryLimit)
                {
                    viewModel.InformationViewModel.Histories.RemoveAt(HistoryLimit);
                }
            });
        }

        private static void AddUserquakeHistory(UserquakeEvaluateEventArgs eventArgs)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                // 開始日時が同じものが存在する場合は上書き更新する
                var histories = viewModel.InformationViewModel.Histories;
                var existItem = histories.FirstOrDefault(e => (e is EPSPUserquakeView view) && (view.EventArgs.StartedAt == eventArgs.StartedAt));

                if (existItem == null)
                {
                    var obj = Factory.WrapEventArgs(eventArgs, client.CalcNowProtocolTime, viewModel.InformationViewModel);
                    histories.Insert(0, obj);
                    if (histories.Count > HistoryLimit)
                    {
                        histories.RemoveAt(HistoryLimit);
                    }
                }
                else if (existItem is EPSPUserquakeView view && view.EventArgs.UpdatedAt < eventArgs.UpdatedAt)
                {
                    view.EventArgs = eventArgs;
                }
            });
        }

        private async static void ReadHistories()
        {
            try
            {
                await ReadAndAddHistories();
            } catch (Exception e)
            {
                SentrySdk.CaptureException(e);
            } finally
            {
                viewModel.InformationViewModel.IsLoading = false;
            }
        }

        private async static Task ReadAndAddHistories() {
            var getTasks = Enumerable.Range(0, 5).Select(i => JsonApi.Client.Get(100, i * 100, Code.Earthquake, Code.Tsunami, Code.EEW, Code.UserquakeEvaluation));
            var results = await Task.WhenAll(getTasks);
            var items = results.SelectMany(e => e);

            var histories = viewModel.InformationViewModel.Histories;

            foreach (var item in items.Reverse())
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
                        Magnitude = $"{quake.Earthquake.Hypocenter.Magnitude.ToString(NumberFormatInfo.InvariantInfo)}",
                        OccuredTime = DateTime.Parse(quake.Earthquake.Time).ToString("d日HH時mm分"),
                        Scale = ConvertScale(quake.Earthquake.MaxScale),
                        Latitude = quake.Earthquake.Hypocenter.Latitude <= -200 ? "" : $"{(quake.Earthquake.Hypocenter.Latitude > 0 ? 'N' : 'S')}{Math.Abs(quake.Earthquake.Hypocenter.Latitude).ToString(NumberFormatInfo.InvariantInfo)}",
                        Longitude = quake.Earthquake.Hypocenter.Longitude <= -200 ? "" : $"{(quake.Earthquake.Hypocenter.Longitude > 0 ? 'E' : 'W')}{Math.Abs(quake.Earthquake.Hypocenter.Longitude).ToString(NumberFormatInfo.InvariantInfo)}",
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

                if (item is EEWDetection eewDetection)
                {

                    var eventArgs = new EPSPEEWTestEventArgs()
                    {
                        IsTest = !(eewDetection.Type == "Full"),
                        ReceivedAt = DateTime.Parse(eewDetection.Time),
                    };
                    if (eventArgs.IsTest) { continue; }
                    AddHistory(eventArgs);
                }

                if (item is EEW eew)
                {
                    var eventArgs = new EPSPEEWEventArgs()
                    {
                        IsTest = false,
                        ReceivedAt = DateTime.Parse(eew.Time),
                        Hypocenter = EEWConverter.GetHypocenterCode(eew.Earthquake.Hypocenter.ReduceName),
                        Areas = eew.Areas.Select(e => e.Pref).Distinct().Select(e => EEWConverter.GetAreaCode(e)).ToArray(),
                    };
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
                        AreaConfidences = evaluation.AreaConfidences.Where(e => e.Value.Confidence >= 0).ToDictionary(e => e.Key.PadLeft(3, '0'), e => new UserquakeEvaluationArea() { AreaCode = e.Key.PadLeft(3, '0'), Confidence = e.Value.Confidence, Count = e.Value.Count } as IUserquakeEvaluationArea)
                    };

                    AddUserquakeHistory(eventArgs);
                }
            }

            viewModel.InformationViewModel.SelectedIndex = 0;
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
                46 => "5弱以上（推定）",
                _ => "不明",
            };
        }

        private static void Window_OnUserquake(object sender, EventArgs e)
        {
            client.SendUserquake();
            MessageBox.Show("地震感知情報を発信しました。", "P2P地震情報", MessageBoxButton.OK, MessageBoxImage.Information);
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

                var epspAreas = Resource.epsp_area.Split('\n').Skip(1).Select(e => e.Split(',')).ToDictionary(e => e[0], e => e[4]);

                viewModel.StatusViewModel.AreapeerText = String.Join("\n", client.AreaPeerDictionary.Where(e => epspAreas.ContainsKey(e.Key)).OrderBy(e => e.Key).Select((area) => $"{epspAreas[area.Key]}: {area.Value}ピア"));

                var mapDrawer = new MapDrawer()
                {
                    MapType = Map.Model.MapType.JAPAN_1024,
                    Areapeers = client.AreaPeerDictionary.Where(e => epspAreas.ContainsKey(e.Key)).Select(e => new Map.Model.Areapeer(e.Key, e.Value)).ToList(),
                };
                var png = mapDrawer.DrawAsPng();

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = png;
                bitmapImage.EndInit();
                viewModel.StatusViewModel.BitmapImage = bitmapImage;
            });
        }

        private static async void Client_StateChanged(object sender, EventArgs e)
        {
            // 未接続、切断中 = 切断アイコン
            var status = ChoiceByState(
                disconnected: "未接続",
                disconnecting: "切断中...",
                connecting: "接続中...",
                connected: "接続済み",
                noConnection: "接続なし"
                );

            // 情報更新 -----
            if (status == "接続済み")
            {
                if (configuration.DisconnectionComplement && DateTime.Now.Subtract(latestConnected).TotalMinutes >= 30)
                {
                    RefreshInformation();
                }

                latestConnected = DateTime.Now;
            }

            // 表示更新 -----
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
            if (client.ReadonlyState.GetType().Name == "ConnectedState")
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

            // 初回の接続済み遷移、または 7 日経過後の遷移だった場合、アップデートチェック
            if (status == "接続済み" && DateTime.Now.Subtract(latestUpdateCheck).TotalDays >= 7)
            {
                var result = await UpdateClient.Check();
                viewModel.ShowUpdateLink = (result.Length > 0) ? Visibility.Visible : Visibility.Hidden;
                latestUpdateCheck = DateTime.Now;
            }
        }

        private static string ChoiceByState(string disconnected, string disconnecting, string connecting, string connected, string noConnection)
        {
            return client.ReadonlyState.GetType().Name switch
            {
                "DisconnectedState" => disconnected,
                "DisconnectingState" => disconnecting,
                "ConnectingState" => connecting,
                _ => client.Connections > 0 ? connected : noConnection
            };
        }
    }
}
