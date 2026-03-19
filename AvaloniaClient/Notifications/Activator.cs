using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;

using Client.App;
using Client.Peer;

using AvaloniaClient.EPSPDataView;
using AvaloniaClient.Services;
using AvaloniaClient.Utils;
using AvaloniaClient.ViewModels;

using Polly;

using System;
using System.Linq;
using System.Threading.Tasks;

namespace AvaloniaClient.Notifications;

public class Activator
{
    private Configuration configuration;

    public static void Select(RootViewModel viewModel, string type, string? receivedAt = null, string? startedAt = null)
    {
        WithRetry(() =>
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                var item = viewModel.InformationViewModel.Histories.First((item) =>
                    (type == "quake" && item is EPSPQuakeView quake && quake.EventArgs.ReceivedAt.ToString() == receivedAt) ||
                    (type == "tsunami" && item is EPSPTsunamiView tsunami && tsunami.EventArgs.ReceivedAt.ToString() == receivedAt) ||
                    (type == "eew" && item is EPSPEEWView eew && eew.EventArgs.ReceivedAt.ToString() == receivedAt) ||
                    (type == "userquake" && item is EPSPUserquakeView userquake && userquake.EventArgs.StartedAt.ToString() == startedAt)
                );
                viewModel.InformationViewModel.SelectedIndex = viewModel.InformationViewModel.Histories.IndexOf(item);
                viewModel.InformationIsSelected = true;
                viewModel.CurrentPage = viewModel.InformationViewModel;
            }).Wait();
        });
    }

    public static void Activate(RootViewModel viewModel, string type, string? receivedAt = null, string? startedAt = null)
    {
        Select(viewModel, type, receivedAt, startedAt);
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
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
        });
    }

    public Activator(Configuration configuration, MediatorContext mediatorContext, RootViewModel viewModel)
    {
        this.configuration = configuration;

        mediatorContext.OnEarthquake += (s, e) => { Task.Run(() => MediatorContext_OnEarthquake(s, e, viewModel)); };
        mediatorContext.OnTsunami += (s, e) => { Task.Run(() => MediatorContext_OnTsunami(s, e, viewModel)); };
        mediatorContext.OnEEW += (s, e) => { Task.Run(() => MediatorContext_OnEEW(s, e, viewModel)); };
        mediatorContext.OnNewUserquakeEvaluation += (s, e) => { Task.Run(() => MediatorContext_OnNewUserquakeEvaluation(s, e, viewModel)); };
    }

    private void MediatorContext_OnEarthquake(object? sender, EPSPQuakeEventArgs e, RootViewModel viewModel)
    {
        if (e.InformationType == QuakeInformationType.Unknown)
        {
            return;
        }

        var earthquakeNotification = configuration.EarthquakeNotification;
        if (!earthquakeNotification.Enabled)
        {
            return;
        }

        var scale = e.InformationType == QuakeInformationType.Destination ? 30 : ScaleConverter.Str2Int(e.Scale);
        var foreign = e.InformationType == QuakeInformationType.Foreign && earthquakeNotification.Foreign;
        if (scale < earthquakeNotification.MinScale && !foreign)
        {
            return;
        }

        if (!earthquakeNotification.Show)
        {
            Select(viewModel, "quake", e.ReceivedAt.ToString());
            return;
        }
        Activate(viewModel, "quake", e.ReceivedAt.ToString());
    }

    private void MediatorContext_OnTsunami(object? sender, EPSPTsunamiEventArgs e, RootViewModel viewModel)
    {
        var tsunamiNotification = configuration.TsunamiNotification;

        if (!tsunamiNotification.Enabled)
        {
            return;
        }

        if (!tsunamiNotification.Show)
        {
            Select(viewModel, "tsunami", e.ReceivedAt.ToString());
            return;
        }
        Activate(viewModel, "tsunami", e.ReceivedAt.ToString());
    }

    private void MediatorContext_OnEEW(object? sender, EPSPEEWEventArgs e, RootViewModel viewModel)
    {
        var eewTestNotification = configuration.EEWTestNotification;

        if (!eewTestNotification.Enabled)
        {
            return;
        }

        if (!eewTestNotification.Show)
        {
            Select(viewModel, "eew", e.ReceivedAt.ToString());
            return;
        }
        Activate(viewModel, "eew", e.ReceivedAt.ToString());
    }

    private void MediatorContext_OnNewUserquakeEvaluation(object? sender, Client.App.Userquake.UserquakeEvaluateEventArgs e, RootViewModel viewModel)
    {
        var userquakeNotification = configuration.UserquakeNotification;

        if (!userquakeNotification.Enabled)
        {
            return;
        }

        if (!userquakeNotification.Show)
        {
            Select(viewModel, "userquake", null, e.StartedAt.ToString());
            return;
        }
        Activate(viewModel, "userquake", null, e.StartedAt.ToString());
    }

    private static void WithRetry(Action a)
    {
        try
        {
            Policy.Handle<Exception>().WaitAndRetry(8, retryAttempt => TimeSpan.FromSeconds(Math.Pow(1.2, retryAttempt - 1) - 0.5)).Execute(a);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"[Activator] Retry exhausted: {e.Message}");
        }
    }
}
