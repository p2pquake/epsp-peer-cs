using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

using FluentAvalonia.UI.Controls;

using AvaloniaClient.Services;
using AvaloniaClient.Views;
using AvaloniaClient.ViewModels;
using AvaloniaClient.EPSPDataView;
using AvaloniaClient.Notifications;
using AvaloniaClient.Utils;

using Client.App;
using Client.App.Userquake;
using Client.Peer;

using JsonApi;

using Map.Controller;
using Map.Model;

using Sentry;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using TsunamiCategory = Client.Peer.TsunamiCategory;

namespace AvaloniaClient;

public partial class App : Application
{
    private MediatorContext? client;
    private RootViewModel? viewModel;
    private Configuration? configuration;
    private Notifier? notifier;
    private Notifications.Activator? activator;
    private Player? player;
    private DateTime latestConnected = DateTime.MaxValue;

    private const int HistoryLimit = 100;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        InitSentry();
        configuration = ConfigurationManager.Configuration;

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            viewModel = new RootViewModel();
            var mainWindow = new MainWindow
            {
                DataContext = viewModel
            };

            mainWindow.OnExit += Window_OnExit;
            mainWindow.OnUserquake += Window_OnUserquake;

            desktop.MainWindow = mainWindow;

            if (configuration.MinimizeAtBoot)
            {
                mainWindow.Opened += (s, e) => mainWindow.Hide();
            }

            desktop.ShutdownRequested += Desktop_ShutdownRequested;
            AppDomain.CurrentDomain.ProcessExit += (s, e) => Disconnect();

            // Load settings
            viewModel.SettingViewModel.LoadFromConfiguration(configuration);
            if (ConfigurationManager.IsFirstBoot)
            {
                viewModel.InformationViewModel.TutorialVisible = true;
                ConfigurationManager.Save();
            }

            // Start P2P connection
            Task.Run(() => BootP2PQuake());
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void BootP2PQuake()
    {
        client = new MediatorContext();
        viewModel!.SettingViewModel.MediatorContext = client;

        client.ConnectionsChanged += Client_ConnectionsChanged;
        client.StateChanged += Client_StateChanged;
        client.OnAreapeers += Client_OnAreapeers;
        client.OnEarthquake += (s, e) => { Task.Run(() => Client_OnEarthquake(s, e)); };
        client.OnTsunami += (s, e) => { Task.Run(() => Client_OnTsunami(s, e)); };
        client.OnEEWTest += (s, e) => { /* Deprecated: replaced by EEW */ };
        client.OnEEW += (s, e) => { Task.Run(() => Client_OnEEW(s, e)); };
        client.OnNewUserquakeEvaluation += (s, e) => { Task.Run(() => Client_OnNewUserquakeEvaluation(s, e)); };
        client.OnUpdateUserquakeEvaluation += (s, e) => { Task.Run(() => Client_OnUpdateUserquakeEvaluation(s, e)); };

        configuration!.OnChangeEPSPConfiguration += (s, e) =>
        {
            ReflectEPSPConfiguration();
        };
        ReflectEPSPConfiguration();

        notifier = new Notifier(configuration!, client, viewModel!);
        activator = new Notifications.Activator(configuration!, client, viewModel!);
        player = new Player(configuration!, client);

        client.Connect();
        Console.Error.WriteLine("[Boot] Skipping P2P connection, reading histories...");
        ReadHistories();
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
            o.SetBeforeSend((sentryEvent, hint) =>
            {
                if (IsExcludeException(sentryEvent.Exception))
                {
                    return null;
                }

                if (sentryEvent.Exception is AggregateException aggregateException)
                {
                    if (aggregateException.InnerExceptions.All(inner => IsExcludeException(inner)))
                    {
                        return null;
                    }
                }
                return sentryEvent;
            });
        });
    }

    private static bool IsExcludeException(Exception? e)
    {
        if (e is not SocketException s)
        {
            return false;
        }

        if (s.SocketErrorCode == SocketError.OperationAborted || s.SocketErrorCode == SocketError.ConnectionAborted || s.SocketErrorCode == SocketError.HostNotFound)
        {
            return true;
        }

        return false;
    }

    public void RefreshInformation()
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            viewModel?.InformationViewModel.Histories.Clear();
        });
        ReadHistories();
    }

    private void ReflectEPSPConfiguration()
    {
        if (client == null || configuration == null) return;
        client.IsPortOpen = configuration.PortOpen;
        client.Port = configuration.Port;
        client.MaxConnections = configuration.PortOpen ? 20 : 4;
        client.UseUPnP = configuration.UseUPnP;
        client.AreaCode = configuration.AreaCode;
    }

    private void Window_OnExit(object? sender, EventArgs e)
    {
        Disconnect();
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
    }

    private async void Window_OnUserquake(object? sender, EventArgs e)
    {
        client?.SendUserquake();
        await ShowUserquakeConfirmation();
    }

    private void Disconnect()
    {
        if (client == null || !client.CanDisconnect) return;

        var sw = new Stopwatch();
        sw.Start();
        client.Disconnect();

        while (sw.ElapsedMilliseconds <= 4000 && !client.CanConnect)
        {
            Thread.Sleep(250);
        }
    }

    private void Desktop_ShutdownRequested(object? sender, ShutdownRequestedEventArgs e)
    {
        Disconnect();
    }

    private async Task ShowUserquakeConfirmation()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow != null)
        {
            var dialog = new TaskDialog
            {
                Title = "P2P地震情報",
                Content = "地震感知情報を発信しました。",
                Buttons = { TaskDialogButton.OKButton },
                XamlRoot = desktop.MainWindow
            };
            await dialog.ShowAsync();
        }
    }

    private void Client_OnEarthquake(object? sender, EPSPQuakeEventArgs e)
    {
        AddHistory(e);
    }

    private void Client_OnTsunami(object? sender, EPSPTsunamiEventArgs e)
    {
        AddHistory(e);
    }

    private void Client_OnEEW(object? sender, EPSPEEWEventArgs e)
    {
        AddHistory(e);
    }

    private void Client_OnNewUserquakeEvaluation(object? sender, UserquakeEvaluateEventArgs e)
    {
        AddUserquakeHistory(e);
    }

    private void Client_OnUpdateUserquakeEvaluation(object? sender, UserquakeEvaluateEventArgs e)
    {
        AddUserquakeHistory(e);
    }

    private async void AddHistory(EventArgs e)
    {
        try
        {
            var obj = Factory.WrapEventArgs(e, client!.CalcNowProtocolTime, viewModel!.InformationViewModel);
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                viewModel.InformationViewModel.Histories.Insert(0, obj!);
                if (viewModel.InformationViewModel.Histories.Count > HistoryLimit)
                {
                    viewModel.InformationViewModel.Histories.RemoveAt(HistoryLimit);
                }
            });
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[AddHistory] Error: {ex}");
        }
    }

    private void AddUserquakeHistory(UserquakeEvaluateEventArgs eventArgs)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            var histories = viewModel!.InformationViewModel.Histories;
            var existItem = histories.FirstOrDefault(e => (e is EPSPUserquakeView view) && (view.EventArgs.StartedAt == eventArgs.StartedAt));

            if (existItem == null)
            {
                var obj = Factory.WrapEventArgs(eventArgs, client!.CalcNowProtocolTime, viewModel.InformationViewModel);
                histories.Insert(0, obj!);
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

    private async void ReadHistories()
    {
        try
        {
            await ReadAndAddHistories();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[ReadHistories] Exception: {ex}");
        }
        finally
        {
            Console.Error.WriteLine($"[ReadHistories] Done, setting IsLoading=false, count={viewModel!.InformationViewModel.Histories.Count}");
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                viewModel!.InformationViewModel.IsLoading = false;
            });
        }
    }

    private async Task ReadAndAddHistories()
    {
        Console.Error.WriteLine("[ReadAndAddHistories] Starting API calls...");
        var allItems = new List<BasicData>();
        for (int i = 0; i < 5; i++)
        {
            try
            {
                var batch = await JsonApi.Client.Get(100, i * 100, Code.Earthquake, Code.Tsunami, Code.EEW, Code.UserquakeEvaluation);
                allItems.AddRange(batch);
                Console.Error.WriteLine($"[ReadAndAddHistories] Batch {i}: {batch.Length} items");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[ReadAndAddHistories] Batch {i} failed: {ex.Message}");
            }
        }
        var items = allItems.AsEnumerable();
        Console.Error.WriteLine($"[ReadAndAddHistories] Got {allItems.Count} items total from API");

        int addedCount = 0;
        foreach (var item in items.Reverse())
        {
            try
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
                    FreeCommentList = (quake.Comments.FreeFormComment != null && quake.Comments.FreeFormComment != "") ?
                       new List<string>() { quake.Comments.FreeFormComment } : new List<string>()
                };
                if (eventArgs.InformationType == QuakeInformationType.ScalePrompt)
                {
                    eventArgs.Depth = "";
                    eventArgs.Magnitude = "";
                }
                if (eventArgs.InformationType == QuakeInformationType.Destination)
                {
                    eventArgs.Scale = "3以上";
                }

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
                        Category = e.Grade switch
                        {
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
                var serial = 1;
                int.TryParse(eew.Issue.Serial, out serial);

                var eventArgs = new EPSPEEWEventArgs()
                {
                    IsTest = false,
                    IsCancelled = eew.Cancelled,
                    IsFollowUp = serial > 1,
                    ReceivedAt = DateTime.Parse(eew.Time),
                    Hypocenter = EEWConverter.GetHypocenterCode(eew.Earthquake?.Hypocenter?.ReduceName),
                    Areas = (eew.Areas ?? Array.Empty<EEWArea>()).Select(e => e.Pref).Distinct().Select(e => EEWConverter.GetAreaCode(e)).ToArray(),
                };
                AddHistory(eventArgs);
            }

            if (item is UserquakeEvaluation evaluation)
            {
                if (evaluation.Confidence <= 0) { continue; }

                var eventArgs = new UserquakeEvaluateEventArgs()
                {
                    StartedAt = DateTime.Parse(evaluation.StartedAt),
                    UpdatedAt = DateTime.Parse(evaluation.UpdatedAt),
                    Count = evaluation.Count,
                    Confidence = evaluation.Confidence,
                    AreaConfidences = evaluation.AreaConfidences.Where(e => e.Value.Confidence >= 0).ToDictionary(e => e.Key.PadLeft(3, '0'), e => new UserquakeEvaluationArea() { AreaCode = e.Key.PadLeft(3, '0'), Confidence = e.Value.Confidence, Count = e.Value.Count } as IUserquakeEvaluationArea)
                };

                AddUserquakeHistory(eventArgs);
            }
            addedCount++;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[ReadAndAddHistories] Error processing item (Code={item.Code}): {ex.Message}");
            }
        }
        Console.Error.WriteLine($"[ReadAndAddHistories] Processed {addedCount} items");

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            viewModel!.InformationViewModel.SelectedIndex = 0;
        });
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

    private void Client_ConnectionsChanged(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            viewModel!.StatusViewModel.Connections = $"{client!.Connections} / {client.MaxConnections} ピア";
        });
    }

    private void Client_OnAreapeers(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            viewModel!.NumberOfPeersLabel = $"{client!.PeerCount} ピア";
            viewModel.StatusViewModel.NumberOfPeers = $"{client.PeerCount} ピア";

            var epspAreas = AreaDataProvider.AreaDictionary;

            viewModel.StatusViewModel.AreapeerText = String.Join("\n", client.AreaPeerDictionary.Where(e => epspAreas.ContainsKey(e.Key)).OrderBy(e => e.Key).Select((area) => $"{epspAreas[area.Key]}: {area.Value}ピア"));

            var mapDrawer = new MapDrawer()
            {
                MapType = MapType.JAPAN_1024,
                Areapeers = client.AreaPeerDictionary.Where(e => epspAreas.ContainsKey(e.Key)).Select(e => new Areapeer(e.Key, e.Value)).ToList(),
            };
            var png = mapDrawer.DrawAsPng();

            viewModel.StatusViewModel.BitmapImage = BitmapHelper.FromStream(png);
        });
    }

    private void Client_StateChanged(object? sender, EventArgs e)
    {
        var status = ChoiceByState(
            disconnected: "未接続",
            disconnecting: "切断中...",
            connecting: "接続中...",
            connected: "接続済み",
            noConnection: "接続なし"
        );

        if (status == "接続済み")
        {
            if (configuration!.DisconnectionComplement && DateTime.Now.Subtract(latestConnected).TotalMinutes >= 30)
            {
                RefreshInformation();
            }
            latestConnected = DateTime.Now;
        }

        var statusSymbol = client!.ReadonlyState.GetType().Name switch
        {
            "DisconnectedState" => Symbol.WifiWarning,
            "DisconnectingState" => Symbol.WifiWarning,
            "ConnectingState" => Symbol.WifiWarning,
            _ => client.Connections > 0 ? Symbol.Wifi4 : Symbol.WifiWarning
        };

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
        if (client!.ReadonlyState.GetType().Name == "ConnectedState")
        {
            portStatus = client.IsPortOpened ? "開放" : "未開放";
        }

        var hasValidKey =
            client.Key != null && !client.Key.IsExpired(client.CalcNowProtocolTime());

        Dispatcher.UIThread.InvokeAsync(() =>
        {
            viewModel!.Status = status;
            viewModel.StatusSymbol = statusSymbol;
            viewModel.PortStatus = $"ポート: {portStatus}";
            viewModel.CanSendUserquake = hasValidKey;

            viewModel.StatusViewModel.PortStatus = portStatus;
            viewModel.StatusViewModel.KeyStatus = hasValidKey ? "有効" : "無効";
            viewModel.StatusViewModel.StatusTitle = statusTitle;
            viewModel.StatusViewModel.StatusDescription = statusDescription;
        });
    }

    private string ChoiceByState(string disconnected, string disconnecting, string connecting, string connected, string noConnection)
    {
        return client!.ReadonlyState.GetType().Name switch
        {
            "DisconnectedState" => disconnected,
            "DisconnectingState" => disconnecting,
            "ConnectingState" => connecting,
            _ => client.Connections > 0 ? connected : noConnection
        };
    }

    // --- Tray icon handlers ---
    private void TrayIcon_Clicked(object? sender, EventArgs e)
    {
        ShowMainWindow();
    }

    private void TrayMenuShow_Click(object? sender, EventArgs e)
    {
        ShowMainWindow();
    }

    private async void TrayMenuShake_Click(object? sender, EventArgs e)
    {
        client?.SendUserquake();
        ShowMainWindow();
        await ShowUserquakeConfirmation();
    }

    private void TrayMenuExit_Click(object? sender, EventArgs e)
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow is MainWindow mainWindow)
        {
            mainWindow.ExitApplication();
        }
    }

    private void ShowMainWindow()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var window = desktop.MainWindow;
            if (window != null)
            {
                window.Show();
                if (window.WindowState == WindowState.Minimized)
                {
                    window.WindowState = WindowState.Normal;
                }
                window.Activate();
            }
        }
    }
}
