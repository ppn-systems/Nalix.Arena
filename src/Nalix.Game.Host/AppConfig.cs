using Nalix.Common.Logging;
using Nalix.Environment.Assemblies;
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
    public static ServerListener Server;
    public static GameDbContext DbContext;

    public static readonly bool IsDebug = Debugger.IsAttached;
    public static readonly ILogger Logger = NLogix.Host.Instance;

    public static string VersionInfo =>
        $"Version {AssemblyInspector.GetAssemblyInformationalVersion()} | {(IsDebug ? "Debug" : "Release")}";

    static AppConfig()
    {
        //if (InitializeDatabase(out GameDbContext? context))
        //{
        //    DbContext = context;
        //    Server = InitializeServer(context);
        //}
        //else
        //{
        //    Logger.Error("Failed to initialize database.");
        //    System.Environment.Exit(1);
        //}

        Server = InitializeServer();
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