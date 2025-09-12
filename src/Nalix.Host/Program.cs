using System;
using System.Threading.Tasks;
using Nalix.Host.Runtime;
using Nalix.Host.Terminals;
using Nalix.Logging;

namespace Nalix.Host;

internal static class Program
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Roslynator", "RCS1163:Unused parameter", Justification = "<Pending>")]
    private static async Task<Int32> Main(String[] args)
    {
        try
        {
            // Compose services manually (no external libs)
            var host = new SimpleHost();

            var consoleReader = new ConsoleReader();
            var shortcuts = new ShortcutManager();
            var server = AppConfig.BuildServer(null, NLogix.Host.Instance);

            var terminal = new TerminalService(consoleReader, shortcuts, server);

            host.AddService(terminal);

            await host.StartAsync().ConfigureAwait(false);

            // Wait until terminal sets ExitEvent (Ctrl+Q double-press)
            terminal.ExitEvent.Wait();

            await host.StopAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
            await host.DisposeAsync();

            return 0;
        }
        catch (Exception ex)
        {
            NLogix.Host.Instance.Fatal("Fatal error in host entry point.", ex);
            return -1;
        }
    }
}
