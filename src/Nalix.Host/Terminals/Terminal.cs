// Copyright (c) 2025 PPN Corporation.

using Nalix.Framework.Injection;
using Nalix.Framework.Tasks;
using Nalix.Logging;
using Nalix.Network.Connection;
using Nalix.Network.Throttling;
using Nalix.Shared.Memory.Pooling;

namespace Nalix.Host.Terminals;

internal sealed class Terminal
{
    #region Const || Fields

    private static readonly System.Threading.Lock ReadLock = new();

    // App lifetime CTS
    private readonly System.Threading.Tasks.Task? _eventLoopTask;
    private readonly System.Threading.CancellationTokenSource _cts = new();

    // Server lifetime
    private System.Threading.Tasks.Task? _serverTask;
    private System.Threading.CancellationTokenSource? _serverCts;

    #endregion Const || Fields

    #region Properties

    public readonly IConsoleReader ConsoleReader;
    public readonly ShortcutManager ShortcutManager;
    public readonly System.Threading.ManualResetEventSlim ExitEvent = new(false);

    #endregion Properties

    #region Ctor

    public Terminal(IConsoleReader consoleReader, ShortcutManager shortcutManager)
    {
        ConsoleReader = consoleReader ?? throw new System.ArgumentNullException(nameof(consoleReader));
        ShortcutManager = shortcutManager ?? throw new System.ArgumentNullException(nameof(shortcutManager));

        InitializeConsole();
        RegisterDefaultShortcuts();

        _eventLoopTask = System.Threading.Tasks.Task.Run(EventLoop);
    }

    #endregion Ctor

    private void START_SERVER()
    {
        if (AppConfig.Listener is null)
        {
            NLogix.Host.Instance.Warn("Server is not initialized.");
            return;
        }

        _serverCts = System.Threading.CancellationTokenSource.CreateLinkedTokenSource(_cts.Token);
        var token = _serverCts.Token;

        AppConfig.Listener!.Activate(token);
    }

    private void STOP_SERVER()
    {
        if (AppConfig.Listener is null)
        {
            NLogix.Host.Instance.Warn("Server is not initialized.");
            return;
        }

        AppConfig.Listener.Deactivate();
    }

    private void SHOW_SHORTCUTS()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder().AppendLine("Available shortcuts:");
        foreach (var (mod, key, desc) in ShortcutManager.GetAllShortcuts())
        {
            System.String modText = FormatModifiers(mod);

            _ = sb.AppendLine($"{modText}{key,-6} → {desc}");
        }
        NLogix.Host.Instance.Info(sb.ToString());
    }

    private void SHOW_REPORT()
    {
        System.Console.WriteLine(InstanceManager.Instance.GetOrCreateInstance<TaskManager>().GenerateReport());
        System.Console.WriteLine(InstanceManager.Instance.GetOrCreateInstance<BufferPoolManager>().GenerateReport());
        System.Console.WriteLine(InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>().GenerateReport());
        System.Console.WriteLine(InstanceManager.Instance.GetOrCreateInstance<ConnectionHub>().GenerateReport());
        System.Console.WriteLine(InstanceManager.Instance.GetOrCreateInstance<ConnectionLimiter>().GenerateReport());
        System.Console.WriteLine(InstanceManager.Instance.GetOrCreateInstance<TokenBucketLimiter>().GenerateReport());
        System.Console.WriteLine(InstanceManager.Instance.GenerateReport());
    }

    #region Initialize

    private void InitializeConsole()
    {
        System.Console.CursorVisible = false;
        System.Console.TreatControlCAsInput = false;
        System.Console.OutputEncoding = System.Text.Encoding.UTF8;
        System.Console.Title = $"Auto ({AppConfig.VersionBanner})";

        System.Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            NLogix.Host.Instance.Warn("Ctrl+C is disabled. Use Ctrl+H for shortcuts, Ctrl+Q to exit.");
        };

        System.AppDomain.CurrentDomain.ProcessExit += (_, __) => ExitEvent.Set();
        System.AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            NLogix.Host.Instance.Error("Unhandled exception: " + e.ExceptionObject);
            ExitEvent.Set();
            _cts.Cancel();
        };

        System.Console.ResetColor();
        NLogix.Host.Instance.Info("Terminal initialized successfully.");
    }

    private void RegisterDefaultShortcuts()
    {
        ShortcutManager.AddOrUpdateShortcut(
            System.ConsoleModifiers.Control,
            System.ConsoleKey.R,
            START_SERVER, "Run server");

        ShortcutManager.AddOrUpdateShortcut(
            System.ConsoleModifiers.Control,
            System.ConsoleKey.H,
            SHOW_SHORTCUTS, "Show shortcuts");

        ShortcutManager.AddOrUpdateShortcut(
            System.ConsoleModifiers.Control,
            System.ConsoleKey.L,
            System.Console.Clear, "Clear screen");

        ShortcutManager.AddOrUpdateShortcut(
            System.ConsoleModifiers.Control,
            System.ConsoleKey.X,
            STOP_SERVER, "Stop server");

        ShortcutManager.AddOrUpdateShortcut(
            System.ConsoleModifiers.Control,
            System.ConsoleKey.M,
            SHOW_REPORT, "Report");

        // Double-press Ctrl+Q within 2s to exit
        System.DateTime lastQuit = System.DateTime.MinValue;
        ShortcutManager.AddOrUpdateShortcut(System.ConsoleModifiers.Control, System.ConsoleKey.Q, () =>
        {
            var now = System.DateTime.UtcNow;
            if ((now - lastQuit).TotalSeconds < 2)
            {
                NLogix.Host.Instance.Info("Exiting gracefully...");
                STOP_SERVER();
                _cts.Cancel();

                if (_eventLoopTask is not null)
                {
                    try { _eventLoopTask.GetAwaiter().GetResult(); }
                    catch (System.OperationCanceledException) { }
                    catch (System.Exception ex) { NLogix.Host.Instance.Error($"EventLoop failed: {ex}"); }
                }

                ExitEvent.Set();
                return;
            }
            lastQuit = now;
            NLogix.Host.Instance.Warn("Press Ctrl+Q again within 2 seconds to exit.");
        }, "Exit (double-press)");
    }

    #endregion Initialize

    #region Private Methods

    private async System.Threading.Tasks.Task EventLoop()
    {
        try
        {
            while (!_cts.IsCancellationRequested)
            {
                if (ConsoleReader.KeyAvailable)
                {
                    System.ConsoleKeyInfo keyInfo;
                    lock (ReadLock)
                    {
                        keyInfo = ConsoleReader.ReadKey(intercept: true);
                    }
                    _ = ShortcutManager.TryExecuteShortcut(keyInfo.Modifiers, keyInfo.Key);
                }
                await System.Threading.Tasks.Task.Delay(10, _cts.Token).ConfigureAwait(false);
            }
        }
        catch (System.OperationCanceledException)
        {
            // normal exit
        }
    }

    /// <summary>
    /// Formats a <see cref="System.ConsoleModifiers"/> into a readable key prefix,
    /// e.g. Ctrl+Shift+, Alt+, or empty if no modifier.
    /// </summary>
    private static System.String FormatModifiers(System.ConsoleModifiers mod)
    {
        if (mod == 0)
        {
            return System.String.Empty;
        }

        var sb = new System.Text.StringBuilder();

        if (mod.HasFlag(System.ConsoleModifiers.Control))
        {
            _ = sb.Append("Ctrl+");
        }

        if (mod.HasFlag(System.ConsoleModifiers.Shift))
        {
            _ = sb.Append("Shift+");
        }

        if (mod.HasFlag(System.ConsoleModifiers.Alt))
        {
            _ = sb.Append("Alt+");
        }

        return sb.ToString();
    }

    #endregion Private Methods

    #region APIs

    public void SetShortcut(
        System.ConsoleModifiers modifiers,
        System.ConsoleKey key, System.Action action, System.String description)
        => ShortcutManager.AddOrUpdateShortcut(modifiers, key, action, description);

    #endregion APIs
}
