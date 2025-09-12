// Copyright (c) 2025

using Nalix.Common.Abstractions;
using Nalix.Framework.Injection;
using Nalix.Framework.Tasks;
using Nalix.Infrastructure.Network;
using Nalix.Logging;
using Nalix.Network.Connection;
using Nalix.Network.Throttling;
using Nalix.Shared.Memory.Pooling;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nalix.Host.Terminals;

/// <summary>
/// Console-driven host service: handles shortcuts and graceful shutdown.
/// </summary>
internal sealed class TerminalService(IConsoleReader reader, ShortcutManager shortcuts, HostListener server) : IActivatable
{
    private static readonly System.Threading.Lock ReadLock = new();
    private readonly IConsoleReader _reader = reader ?? throw new ArgumentNullException(nameof(reader));
    private readonly HostListener _server = server ?? throw new ArgumentNullException(nameof(server));
    private readonly ShortcutManager _shortcuts = shortcuts ?? throw new ArgumentNullException(nameof(shortcuts));

    private CancellationToken _hostToken;
    private Task? _loopTask;
    private volatile Boolean _started;
    private volatile Boolean _disposed;

    // double-press tracking
    private readonly Stopwatch _quitWatch = Stopwatch.StartNew();
    private Int64 _lastQuitTick = -1; // ticks from Stopwatch

    public ManualResetEventSlim ExitEvent { get; } = new(false); // still available for external waiters

    public void Activate(CancellationToken token)
    {
        if (_started)
        {
            return;
        }

        _hostToken = token;

        InitializeConsole(); // events + console config
        RegisterDefaultShortcuts();

        _loopTask = Task.Run(EventLoop, token);
        _started = true;
        NLogix.Host.Instance.Info("[TERMINAL] started");
    }

    public void Deactivate(CancellationToken token)
    {
        if (!_started)
        {
            return;
        }

        // Signal exit and wait for loop to finish (best effort)
        this.ExitEvent.Set();

        if (_loopTask is not null)
        {
            try { _loopTask.Wait(TimeSpan.FromSeconds(2), token); }
            catch { /* ignore */ }
        }

        UnsubscribeConsoleEvents();
        _started = false;
        NLogix.Host.Instance.Info("[TERMINAL] stopped");
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        try { UnsubscribeConsoleEvents(); } catch { }
        try { this.ExitEvent.Dispose(); } catch { }
    }

    // ===== console init & events =====

    private void InitializeConsole()
    {
        Console.CursorVisible = false;
        Console.TreatControlCAsInput = false;
        Console.OutputEncoding = Encoding.UTF8;
        Console.Title = $"Auto ({AppConfig.VersionBanner})";

        Console.CancelKeyPress += OnCancelKeyPress;
        AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

        Console.ResetColor();
        NLogix.Host.Instance.Info("Terminal initialized successfully.");
    }

    private void UnsubscribeConsoleEvents()
    {
        Console.CancelKeyPress -= OnCancelKeyPress;
        AppDomain.CurrentDomain.ProcessExit -= OnProcessExit;
        AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;
    }

    private void OnCancelKeyPress(Object? sender, ConsoleCancelEventArgs e)
    {
        e.Cancel = true;
        NLogix.Host.Instance.Warn("Ctrl+C is disabled. Use Ctrl+H for shortcuts, Ctrl+Q to exit.");
    }

    private void OnProcessExit(Object? _, EventArgs __)
        => this.ExitEvent.Set();

    private void OnUnhandledException(Object? _, UnhandledExceptionEventArgs e)
    {
        NLogix.Host.Instance.Error("Unhandled exception: " + e.ExceptionObject);
        this.ExitEvent.Set();
    }

    // ===== shortcuts =====

    private void RegisterDefaultShortcuts()
    {
        _shortcuts.AddOrUpdateShortcut(ConsoleModifiers.Control, ConsoleKey.R, () => _server.Activate(_hostToken), "Run server");

        _shortcuts.AddOrUpdateShortcut(ConsoleModifiers.Control, ConsoleKey.X, () => _server.Deactivate(), "Stop server");

        _shortcuts.AddOrUpdateShortcut(ConsoleModifiers.Control, ConsoleKey.L, Console.Clear, "Clear screen");

        _shortcuts.AddOrUpdateShortcut(ConsoleModifiers.Control, ConsoleKey.M, SHOW_REPORT, "Report");

        _shortcuts.AddOrUpdateShortcut(ConsoleModifiers.Control, ConsoleKey.H, SHOW_SHORTCUTS, "Show shortcuts");

        _shortcuts.AddOrUpdateShortcut(ConsoleModifiers.Control, ConsoleKey.Q, () =>
        {
            if (TryHandleQuit())
            {
                return;
            }

            NLogix.Host.Instance.Warn("Press Ctrl+Q again within 2 seconds to exit.");
        }, "Exit (double-press)");
    }

    private Boolean TryHandleQuit()
    {
        const Double windowSeconds = 2.0;
        Int64 now = _quitWatch.ElapsedTicks;

        if (_lastQuitTick >= 0)
        {
            Double delta = (now - _lastQuitTick) / (Double)Stopwatch.Frequency;
            if (delta <= windowSeconds)
            {
                NLogix.Host.Instance.Info("Exiting gracefully...");
                _server.Deactivate();
                this.ExitEvent.Set();
                return true;
            }
        }

        _lastQuitTick = now;
        return false;
    }

    // ===== report & shortcuts helpers =====

    private void SHOW_SHORTCUTS()
    {
        var sb = new StringBuilder().AppendLine("Available shortcuts:");
        foreach (var (mod, key, desc) in _shortcuts.GetAllShortcuts())
        {
            sb.AppendLine($"{FormatModifiers(mod)}{key,-6} → {desc}");
        }
        NLogix.Host.Instance.Info(sb.ToString());
    }

    private void SHOW_REPORT()
    {
        Console.WriteLine(InstanceManager.Instance.GetOrCreateInstance<TaskManager>().GenerateReport());
        Console.WriteLine(InstanceManager.Instance.GetOrCreateInstance<BufferPoolManager>().GenerateReport());
        Console.WriteLine(InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>().GenerateReport());
        Console.WriteLine(InstanceManager.Instance.GetOrCreateInstance<ConnectionHub>().GenerateReport());
        Console.WriteLine(InstanceManager.Instance.GetOrCreateInstance<ConnectionLimiter>().GenerateReport());
        Console.WriteLine(InstanceManager.Instance.GetOrCreateInstance<TokenBucketLimiter>().GenerateReport());
        Console.WriteLine(InstanceManager.Instance.GenerateReport());
    }

    private static String FormatModifiers(ConsoleModifiers mod)
    {
        if (mod == 0)
        {
            return String.Empty;
        }

        var sb = new StringBuilder();
        if (mod.HasFlag(ConsoleModifiers.Control))
        {
            sb.Append("Ctrl+");
        }

        if (mod.HasFlag(ConsoleModifiers.Shift))
        {
            sb.Append("Shift+");
        }

        if (mod.HasFlag(ConsoleModifiers.Alt))
        {
            sb.Append("Alt+");
        }

        return sb.ToString();
    }

    // ===== event loop =====

    private async Task EventLoop()
    {
        try
        {
            while (!_hostToken.IsCancellationRequested && !this.ExitEvent.IsSet)
            {
                if (_reader.KeyAvailable)
                {
                    ConsoleKeyInfo keyInfo;
                    lock (ReadLock) { keyInfo = _reader.ReadKey(intercept: true); }
                    _ = _shortcuts.TryExecuteShortcut(keyInfo.Modifiers, keyInfo.Key);
                }
                await Task.Delay(10, _hostToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) { /* normal */ }
        catch (Exception ex) { NLogix.Host.Instance.Error("[TERMINAL] loop faulted", ex); }
    }
}
