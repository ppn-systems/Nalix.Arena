using Nalix.Application.Operations.Security;
using Nalix.Common.Logging.Abstractions;
using Nalix.Common.Logging.Models;
using Nalix.Communication;
using Nalix.Host.Assemblies;
using Nalix.Infrastructure.Database;
using Nalix.Infrastructure.Network;
using Nalix.Logging;
using Nalix.Logging.Sinks.Console;
using Nalix.Network.Dispatch;
using Nalix.Shared.Injection;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Nalix.Host;

internal static class AppConfig
{
    private static readonly Lazy<GameDbContext> s_db = new(
        CreateDbContextCore,
        LazyThreadSafetyMode.ExecutionAndPublication);

    /// <summary>
    /// Banner phiên bản cho log/console.
    /// </summary>
    public static String VersionBanner
        => $"Nalix.Host {AssemblyInspector.GetAssemblyInformationalVersion()} | {(Debugger.IsAttached ? "Debug" : "Release")}";

    /// <summary>
    /// Trả về DbContext đã khởi tạo (lazy). Không Dispose ở đây.
    /// </summary>
    public static GameDbContext DbContext => s_db.Value;

    public static ServerListener Listener;

    [UnconditionalSuppressMessage("Trimming",
        "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' " +
        "require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    static AppConfig()
    {
        Registry.Load();

        NLogix log = new(cfg => cfg.AddTarget(new ConsoleLogTarget())
                             .SetMinLevel(LogLevel.Information));

        if (InstanceManager.Instance.GetExistingInstance<ILogger>() == null)
        {
            InstanceManager.Instance.Register<ILogger>(log);
        }

        Listener = BuildServer(null, log);
    }

    /// <summary>
    /// Thử khởi tạo database context ngay-lập-tức (không ném ra ngoài).
    /// Hữu ích cho health-check hoặc warm-up.
    /// </summary>
    public static Boolean TryInitializeDatabase([NotNullWhen(true)] out GameDbContext? ctx)
    {
        try
        {
            ctx = s_db.Value;
            return true;
        }
        catch (Exception ex)
        {
            NLogix.Host.Instance.Error(Evt("DB_INIT_FAIL") + "Failed to initialize database.", ex);
            ctx = null;
            return false;
        }
    }

    /// <summary>
    /// Tạo sẵn ServerListener đã cấu hình protocol & pipeline — KHÔNG gọi Start().
    /// </summary>
    public static ServerListener BuildServer(GameDbContext? dbOverride = null, ILogger? loggerOverride = null)
    {
        var log = loggerOverride ?? NLogix.Host.Instance;
        var db = dbOverride ?? (s_db.IsValueCreated ? s_db.Value : null); // cho phép null-db cho một số handler

        var channel = BuildDispatchChannel(log, db);
        channel.Activate();
        var protocol = new ServerProtocol(channel);
        return new ServerListener(protocol);
    }

    /// <summary>
    /// Cho phép phần khác build riêng channel.
    /// </summary>
    public static PacketDispatchChannel BuildDispatchChannel(ILogger log, GameDbContext? dbOrNull)
    {
        return new PacketDispatchChannel(cfg => cfg
            .WithLogging(log)
            .WithErrorHandling((exception, command)
                => log.Error(Evt("DISPATCH_ERR") + $"Error handling command: {command}", exception))
            // Register handlers tại một chỗ, dễ test và kiểm soát thứ tự
            .WithHandler(() => new HandshakeOps())
        //.WithHandler(() => new AccountOps(dbOrNull))
        //.WithHandler(() => new PasswordOps(dbOrNull))
        );
    }

    #region internals

    private static GameDbContext CreateDbContextCore()
    {
        try
        {
            var ctx = new AutoDbContextFactory().CreateDbContext([]);
            _ = ctx.Database.EnsureCreated();
            NLogix.Host.Instance.Info(Evt("DB_INIT_OK") + "Database initialized successfully.");
            return ctx;
        }
        catch
        {
            // Log chi tiết ở TryInitializeDatabase; ở đây để nguyên stack cho caller nếu cần.
            throw;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static String Evt(String name) => $"[{name}] ";

    #endregion
}