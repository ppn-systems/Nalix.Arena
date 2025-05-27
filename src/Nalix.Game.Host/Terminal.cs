using Nalix.Network.Snapshot;
using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nalix.Game.Host;

internal class Terminal
{
    public static readonly CancellationTokenSource CTokenSrc = new();
    public static readonly ManualResetEventSlim ExitEvent = new(false);

    static Terminal()
    {
        Console.CursorVisible = false;
        Console.TreatControlCAsInput = false;
        Console.OutputEncoding = Encoding.UTF8;
        Console.Title = $"Auto ({AppConfig.VersionInfo})";

        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            AppConfig.Logger.Warn("Ctrl+C is disabled. Use Ctrl+H to show shortcuts.");
        };
        AppDomain.CurrentDomain.ProcessExit += (s, e) => ExitEvent.Set();
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            AppConfig.Logger.Error("Unhandled exception: " + e.ExceptionObject);
            ExitEvent.Set();
        };

        Console.Clear();
        Console.ResetColor();

        Task.Run(async () =>
        {
            while (!ExitEvent.IsSet)
            {
                if (Console.KeyAvailable)
                {
                    var keyInfo = Console.ReadKey(true);
                    if (keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control) &&
                        Shortcuts.TryGetValue(keyInfo.Key, out var action))
                    {
                        action?.Invoke();
                    }
                }
                await Task.Delay(10);
            }
        });

        AppConfig.Logger.Info("Terminal initialized successfully.");
    }

    private static readonly ConcurrentDictionary<ConsoleKey, Action> Shortcuts = new()
    {
        [ConsoleKey.H] = () => ShowShortcuts(),
        [ConsoleKey.Q] = () =>
        {
            AppConfig.Logger.Info("Ctrl+Q pressed: Initiating graceful shutdown...");
            if (AppConfig.Server != null)
            {
                try
                {
                    if (!AppConfig.Server.IsListening)
                        return;

                    AppConfig.Server.StopListening();
                    AppConfig.Logger.Info("Server stopped.");
                }
                catch (Exception ex)
                {
                    AppConfig.Logger.Error($"Error during shutdown: {ex.Message}");
                }
            }

            CTokenSrc.Cancel();
            ExitEvent.Set();
            System.Environment.Exit(0);
        },
        [ConsoleKey.R] = () =>
        {
            if (AppConfig.Server == null)
            {
                AppConfig.Logger.Warn("Server is not initialized.");
                return;
            }

            ThreadPool.QueueUserWorkItem(_ => AppConfig.Server.StartListeningAsync(CTokenSrc.Token));
        },
        [ConsoleKey.P] = () =>
        {
            if (AppConfig.Server == null)
            {
                AppConfig.Logger.Warn("Server is not initialized.");
                return;
            }

            Task.Run(() => AppConfig.Server.StopListening());
        },
        [ConsoleKey.S] = () =>
        {
            if (AppConfig.Server == null)
            {
                AppConfig.Logger.Warn("Server is not initialized.");
                return;
            }

            ListenerSnapshot snapshot = AppConfig.Server.GetSnapshot();

            AppConfig.Logger.Info(
                $"Server status: {(snapshot.IsListening ? "Running" : "Stopped")}" +
                $"\nAddress: {snapshot.Address} - Port: {snapshot.Port} - Dispose: {snapshot.IsDisposed}" +
                $"\nSocketStatus: {snapshot.ListenerSocketStatus}");
        }
    };

    public static void SetShortcut(ConsoleKey key, Action action) => Shortcuts[key] = action;

    private static string GetShortcutDescription(ConsoleKey key)
    {
        return key switch
        {
            ConsoleKey.H => "Show shortcuts",
            ConsoleKey.Q => "Exit",
            ConsoleKey.R => "Run server",
            ConsoleKey.P => "Stop server",
            _ => "Custom action"
        };
    }

    private static void ShowShortcuts()
    {
        string indent = new(' ', 45);
        StringBuilder builder = new();
        builder.AppendLine("Available shortcuts:");
        foreach (var shortcut in Shortcuts)
        {
            builder.AppendLine(
                $"{indent}Ctrl+{shortcut.Key}".PadRight(10) + $"→ {GetShortcutDescription(shortcut.Key)}");
        }
        AppConfig.Logger.Info(builder.ToString());
    }
}