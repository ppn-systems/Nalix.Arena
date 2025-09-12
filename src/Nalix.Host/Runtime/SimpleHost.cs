using Nalix.Common.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Nalix.Host.Runtime;

/// <summary>
/// Tiny host that manages a set of IHostService instances without external libs.
/// </summary>
public sealed class SimpleHost : IAsyncDisposable
{
    private readonly List<IActivatable> _servicesSync = [];
    private readonly List<IAsyncActivatable> _servicesAsync = [];
    private readonly CancellationTokenSource _cts = new();

    private volatile Boolean _started;
    private volatile Boolean _stopping;

    public CancellationToken Token => _cts.Token;

    public SimpleHost AddService(IAsyncActivatable service)
    {
        ArgumentNullException.ThrowIfNull(service);
        _servicesAsync.Add(service);
        return this;
    }

    public SimpleHost AddService(IActivatable service)
    {
        ArgumentNullException.ThrowIfNull(service);
        _servicesSync.Add(service);
        return this;
    }

    /// <summary>Start all services sequentially (fail-fast on first error).</summary>
    public async Task StartAsync()
    {
        if (_started)
        {
            return;
        }

        foreach (var s in _servicesAsync)
        {
            await s.ActivateAsync(_cts.Token).ConfigureAwait(false);
        }
        foreach (var s in _servicesSync)
        {
            s.Activate(_cts.Token);
        }
        _started = true;
    }

    /// <summary>Stop all services in reverse order.</summary>
    public async Task StopAsync(TimeSpan? timeout = null)
    {
        if (_stopping)
        {
            return;
        }

        _stopping = true;

        using var linked = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token);
        if (timeout is { } t && t > TimeSpan.Zero)
        {
            linked.CancelAfter(t);
        }

        for (Int32 i = _servicesAsync.Count - 1; i >= 0; i--)
        {
            try { await _servicesAsync[i].DeactivateAsync(linked.Token).ConfigureAwait(false); }
            catch (OperationCanceledException) { /* best effort */ }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception during service deactivation: {ex}");
            }
        }

        for (Int32 i = _servicesSync.Count - 1; i >= 0; i--)
        {
            try { _servicesSync[i].Deactivate(linked.Token); }
            catch (OperationCanceledException) { /* best effort */ }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception during service deactivation: {ex}");
            }
        }

        _cts.Cancel();
    }

    public async ValueTask DisposeAsync()
    {
        try { await StopAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false); }
        catch { /* ignore */ }

        foreach (var s in _servicesAsync)
        {
            if (s is IAsyncDisposable asyncDisp) { try { await asyncDisp.DisposeAsync().ConfigureAwait(false); } catch { } }
            else if (s is IDisposable disp) { try { disp.Dispose(); } catch { } }
        }

        foreach (var s in _servicesSync)
        {
            if (s is IDisposable disp) { try { disp.Dispose(); } catch { } }
        }

        _cts.Dispose();
    }
}
