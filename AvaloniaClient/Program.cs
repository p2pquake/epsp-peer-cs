using Avalonia;

using System;
using System.IO;
using System.IO.Pipes;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AvaloniaClient;

public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
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
            // 既に起動しているプロセスにShowコマンドを送信
            SendShowCommand();
            return;
        }

        Task.Run(RunNamedPipeServer);

        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    private static void SendShowCommand()
    {
        using var pipe = new NamedPipeClientStream(".", IPC.Const.Name, PipeDirection.Out, PipeOptions.CurrentUserOnly);
        try
        {
            pipe.Connect(500);
            using var writer = new StreamWriter(pipe) { AutoFlush = true };
            writer.WriteLine(JsonSerializer.Serialize(new IPC.Message(IPC.Method.Show)));
        }
        catch (TimeoutException)
        {
            Console.Error.WriteLine("P2P地震情報は既に起動しています。");
        }
    }

    private static void RunNamedPipeServer()
    {
        while (true)
        {
            try
            {
                using var pipe = new NamedPipeServerStream(IPC.Const.Name, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.CurrentUserOnly);
                pipe.WaitForConnection();

                using var reader = new StreamReader(pipe);
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    var message = JsonSerializer.Deserialize<IPC.Message>(line);
                    if (message == null) continue;

                    switch (message.Method)
                    {
                        case IPC.Method.Show:
                            ShowMainWindow();
                            break;
                        case IPC.Method.Exit:
                            ExitApplication();
                            return;
                        default:
                            Console.Error.WriteLine($"不明なIPCコマンド: {message.Method}");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[IPC] Error: {ex.Message}");
            }
        }
    }

    private static void ShowMainWindow()
    {
        Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (Avalonia.Application.Current?.ApplicationLifetime
                is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
            {
                var window = desktop.MainWindow;
                if (window != null)
                {
                    window.Show();
                    if (window.WindowState == Avalonia.Controls.WindowState.Minimized)
                    {
                        window.WindowState = Avalonia.Controls.WindowState.Normal;
                    }
                    window.Activate();
                }
            }
        });
    }

    private static void ExitApplication()
    {
        Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (Avalonia.Application.Current?.ApplicationLifetime
                is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
            {
                if (desktop.MainWindow is AvaloniaClient.Views.MainWindow mainWindow)
                {
                    mainWindow.ExitApplication();
                }
                else
                {
                    desktop.Shutdown();
                }
            }
        });
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
}
