using Nalix.Application.Operations.Security;
using Nalix.Common.Logging.Abstractions;
using Nalix.Communication;
using Nalix.Host.Assemblies;
using Nalix.Infrastructure.Database;
using Nalix.Infrastructure.Network;
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
    // Lazy thread-safe, không side-effect ngoài khởi tạo
    private static readonly Lazy<ILogger> s_log = new(
        () => InstanceManager.Instance.GetExistingInstance<ILogger>()
              ?? throw new InvalidOperationException("ILogger is not registered."),
        LazyThreadSafetyMode.ExecutionAndPublication);

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

    [UnconditionalSuppressMessage("Trimming",
        "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' " +
        "require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    static AppConfig()
    {
        Registry.Load();
        if (InstanceManager.Instance.GetExistingInstance<ILogger>() == null)
        {
            InstanceManager.Instance.Register<ILogger>(Logging.NLogix.Host.Instance);
        }
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
            s_log.Value.Error(Evt("DB_INIT_FAIL"), "Failed to initialize database.", ex);
            ctx = null;
            return false;
        }
    }

    /// <summary>
    /// Tạo sẵn ServerListener đã cấu hình protocol & pipeline — KHÔNG gọi Start().
    /// </summary>
    public static ServerListener BuildServer(GameDbContext? dbOverride = null, ILogger? loggerOverride = null)
    {
        var log = loggerOverride ?? s_log.Value;
        var db = dbOverride ?? (s_db.IsValueCreated ? s_db.Value : null); // cho phép null-db cho một số handler

        var channel = BuildDispatchChannel(log, db);
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
                => log.Error(Evt("DISPATCH_ERR"), $"Error handling command: {command}", exception))
            // Register handlers tại một chỗ, dễ test và kiểm soát thứ tự
            .WithHandler(() => new HandshakeOps())
            .WithHandler(() => new AccountOps(dbOrNull))
            .WithHandler(() => new PasswordOps(dbOrNull))
        );
    }

    #region internals

    private static GameDbContext CreateDbContextCore()
    {
        var log = s_log.Value;
        try
        {
            var ctx = new AutoDbContextFactory().CreateDbContext([]);
            _ = ctx.Database.EnsureCreated();
            log.Info(Evt("DB_INIT_OK"), "Database initialized successfully.");
            return ctx;
        }
        catch
        {
            // Log chi tiết ở TryInitializeDatabase; ở đây để nguyên stack cho caller nếu cần.
            throw;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static String Evt(String name) => $"[{name}]";

    #endregion
}