using Nalix.Assemblies;
using Nalix.Common.Logging;
using Nalix.Game.Infrastructure.Database;
using Nalix.Game.Infrastructure.Network;
using Nalix.Logging;
using Nalix.Network.Dispatch;
using Nalix.Network.Package;
using Nalix.Shared.Memory.Buffers;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Nalix.Game.Host;

internal static class AppConfig
{
    private static ServerListener? _server;
    private static GameDbContext? _context;

    public static readonly ILogger Logger = NLogix.Host.Instance;

    public static string VersionInfo =>
        $"Version {AssemblyInspector.GetAssemblyInformationalVersion()} | {(Debugger.IsAttached ? "Debug" : "Release")}";

    /// <summary>
    /// Server được khởi tạo khi gọi lần đầu
    /// </summary>
    public static ServerListener Server
    {
        get
        {
            if (_server == null)
            {
                Logger.Warn("Lazy-initializing Server...");
                _server = InitializeServer();
            }
            return _server;
        }
    }

    /// <summary>
    /// Database context được khởi tạo khi gọi lần đầu
    /// </summary>
    public static GameDbContext DbContext
    {
        get
        {
            if (_context == null)
            {
                if (!InitializeDatabase(out _context))
                {
                    Logger.Error("Database failed to initialize.");
                    Environment.Exit(1); // or throw
                }
            }
            return _context;
        }
    }

    public static bool InitializeDatabase([NotNullWhen(true)] out GameDbContext? context)
    {
        try
        {
            context = new AutoDbContextFactory().CreateDbContext([]);
            context.Database.EnsureCreated();
            Logger.Info("Database initialized successfully.");
            return true;
        }
        catch (Exception ex)
        {
            context = null;
            Logger.Error("Failed to initialize database.", ex);
            return false;
        }
    }

    public static ServerListener InitializeServer()
    {
        return new ServerListener(
               new ServerProtocol(new PacketDispatch<Packet>(cfg => cfg
                   .WithLogging(Logger)
                   .WithErrorHandling((exception, command) =>
                        Logger.Error($"Error handling command: {command}", exception))
        //.WithHandler(() => new AccountService(context))
        )), new BufferAllocator(), Logger);
    }
}