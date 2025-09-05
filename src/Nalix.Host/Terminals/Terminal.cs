// Copyright (c) 2025 PPN Corporation.

using Nalix.Host.Extensions;
using Nalix.Logging;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nalix.Host.Terminals;

internal sealed class Terminal
{
    private static readonly Lock ReadLock = new();

    // App lifetime CTS
    private readonly Task? _eventLoopTask;
    private readonly CancellationTokenSource _cts = new();

    // Server lifetime
    private Task? _serverTask;
    private CancellationTokenSource? _serverCts;

    public readonly IConsoleReader ConsoleReader;
    public readonly ShortcutManager ShortcutManager;
    public readonly ManualResetEventSlim ExitEvent = new(false);

    public Terminal(IConsoleReader consoleReader, ShortcutManager shortcutManager)
    {
        ConsoleReader = consoleReader ?? throw new ArgumentNullException(nameof(consoleReader));
        ShortcutManager = shortcutManager ?? throw new ArgumentNullException(nameof(shortcutManager));

        InitializeConsole();
        RegisterDefaultShortcuts();

        _eventLoopTask = Task.Run(EventLoop);
    }

    private void InitializeConsole()
    {
        Console.CursorVisible = false;
        Console.TreatControlCAsInput = false;
        Console.OutputEncoding = Encoding.UTF8;
        Console.Title = $"Auto ({AppConfig.VersionBanner})";

        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            NLogix.Host.Instance.Warn("Ctrl+C is disabled. Use Ctrl+H for shortcuts, Ctrl+Q to exit.");
        };

        AppDomain.CurrentDomain.ProcessExit += (_, __) => ExitEvent.Set();
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            NLogix.Host.Instance.Error("Unhandled exception: " + e.ExceptionObject);
            ExitEvent.Set();
            _cts.Cancel();
        };

        Console.ResetColor();
        NLogix.Host.Instance.Info("Terminal initialized successfully.");
    }

    private void RegisterDefaultShortcuts()
    {
        ShortcutManager.AddOrUpdateShortcut(ConsoleModifiers.Control, ConsoleKey.H, ShowShortcuts, "Show shortcuts");
        ShortcutManager.AddOrUpdateShortcut(ConsoleModifiers.Control, ConsoleKey.R, StartServer, "Run server");
        ShortcutManager.AddOrUpdateShortcut(ConsoleModifiers.Control, ConsoleKey.L, Console.Clear, "Clear screen");
        ShortcutManager.AddOrUpdateShortcut(ConsoleModifiers.Control, ConsoleKey.X, () => StopServerInternalAsync().Forget(), "Stop server");

        // Double-press Ctrl+Q within 2s to exit
        DateTime lastQuit = DateTime.MinValue;
        ShortcutManager.AddOrUpdateShortcut(ConsoleModifiers.Control, ConsoleKey.Q, () =>
        {
            var now = DateTime.UtcNow;
            if ((now - lastQuit).TotalSeconds < 2)
            {
                NLogix.Host.Instance.Info("Exiting gracefully...");
                StopServerInternalAsync().GetAwaiter().GetResult();
                _cts.Cancel();

                if (_eventLoopTask is not null)
                {
                    try { _eventLoopTask.GetAwaiter().GetResult(); }
                    catch (OperationCanceledException) { }
                    catch (Exception ex) { NLogix.Host.Instance.Error($"EventLoop failed: {ex}"); }
                }

                ExitEvent.Set();
                return;
            }
            lastQuit = now;
            NLogix.Host.Instance.Warn("Press Ctrl+Q again within 2 seconds to exit.");
        }, "Exit (double-press)");
    }

    private void StartServer()
    {
        if (AppConfig.Listener is null)
        {
            NLogix.Host.Instance.Warn("Server is not initialized.");
            return;
        }

        _serverCts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token);
        var token = _serverCts.Token;

        _serverTask = Task.Run(async () =>
        {
            try
            {
                await AppConfig.Listener!.ActivateAsync(token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                NLogix.Host.Instance.Debug("Server activation canceled (shutdown).");
            }
            catch (ObjectDisposedException) when (token.IsCancellationRequested)
            {
                NLogix.Host.Instance.Debug("Server listener disposed during shutdown.");
            }
            catch (Exception ex)
            {
                NLogix.Host.Instance.Error($"Unhandled server error: {ex}");
            }
        });
    }

    private async Task StopServerInternalAsync()
    {
        if (AppConfig.Listener is null)
        {
            NLogix.Host.Instance.Warn("Server is not initialized.");
            return;
        }

        try
        {
            if (AppConfig.Listener.IsListening)
            {
                await AppConfig.Listener.DeactivateAsync().ConfigureAwait(false);
            }

            var cts = Interlocked.Exchange(ref _serverCts, null);
            cts?.Cancel();

            var task = Interlocked.Exchange(ref _serverTask, null);
            if (task is not null)
            {
                try { await task.ConfigureAwait(false); }
                catch (OperationCanceledException) { }
            }
        }
        catch (OperationCanceledException)
        {
            NLogix.Host.Instance.Debug("Shutdown canceled (already stopping).");
        }
        catch (ObjectDisposedException)
        {
            NLogix.Host.Instance.Debug("Listener already disposed during shutdown.");
        }
        catch (Exception ex)
        {
            NLogix.Host.Instance.Error($"Error during shutdown: {ex}");
        }
    }

    private async Task EventLoop()
    {
        try
        {
            while (!_cts.IsCancellationRequested)
            {
                if (ConsoleReader.KeyAvailable)
                {
                    ConsoleKeyInfo keyInfo;
                    lock (ReadLock)
                    {
                        keyInfo = ConsoleReader.ReadKey(intercept: true);
                    }
                    _ = ShortcutManager.TryExecuteShortcut(keyInfo.Modifiers, keyInfo.Key);
                }
                await Task.Delay(10, _cts.Token).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            // normal exit
        }
    }

    private void ShowShortcuts()
    {
        var sb = new StringBuilder().AppendLine("Available shortcuts:");
        foreach (var (mod, key, desc) in ShortcutManager.GetAllShortcuts())
        {
            String modText = mod == 0 ? "" :
                $"{(mod.HasFlag(ConsoleModifiers.Control) ? "Ctrl+" : "")}" +
                $"{(mod.HasFlag(ConsoleModifiers.Shift) ? "Shift+" : "")}" +
                $"{(mod.HasFlag(ConsoleModifiers.Alt) ? "Alt+" : "")}";

            _ = sb.AppendLine($"{modText}{key,-6} → {desc}");
        }
        NLogix.Host.Instance.Info(sb.ToString());
    }

    public void SetShortcut(ConsoleModifiers modifiers, ConsoleKey key, Action action, String description)
        => ShortcutManager.AddOrUpdateShortcut(modifiers, key, action, description);
}
