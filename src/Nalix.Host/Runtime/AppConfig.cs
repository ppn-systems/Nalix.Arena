using Nalix.Application.Operations.Security;
using Nalix.Common.Logging.Abstractions;
using Nalix.Common.Packets.Abstractions;
using Nalix.Common.Packets.Models;
using Nalix.Communication;
using Nalix.Communication.Collections;
using Nalix.Communication.Enums;
using Nalix.Communication.Extensions;
using Nalix.Framework.Injection;
using Nalix.Host.Assemblies;
using Nalix.Infrastructure.Database;
using Nalix.Infrastructure.Network;
using Nalix.Infrastructure.Repositories;
using Nalix.Logging;
using Nalix.Network.Dispatch;
using Nalix.Network.Middleware.Inbound;
using Nalix.Network.Middleware.Outbound;
using Nalix.Shared.Extensions;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Nalix.Host.Runtime;

internal static class AppConfig
{
    /// <summary>
    /// Banner phiên bản cho log/console.
    /// </summary>
    public static String VersionBanner
        => $"Nalix.Host {AssemblyInspector.GetAssemblyInformationalVersion()} | {(Debugger.IsAttached ? "Debug" : "Release")}";

    public static HostListener Listener;

    public static DbConnectionFactory DbFactory
        => InstanceManager.Instance.GetOrCreateInstance<DbConnectionFactory>();

    [UnconditionalSuppressMessage("Trimming",
        "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' " +
        "require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    static AppConfig()
    {
        Registry.Load();

        if (InstanceManager.Instance.GetExistingInstance<ILogger>() == null)
        {
            InstanceManager.Instance.Register<ILogger>(NLogix.Host.Instance);
        }

        SelfTestPackets();

        DbConnectionFactory factory = InstanceManager.Instance.GetOrCreateInstance<DbConnectionFactory>();

        DbInitializer.EnsureDatabaseInitializedAsync(factory).GetAwaiter().GetResult();

        CredentialsRepository credentials = new(factory);

        PacketDispatchChannel channel = new(cfg => cfg
            .WithLogging(NLogix.Host.Instance)
            .WithErrorHandling((exception, command)
                => NLogix.Host.Instance.Error($"Error handling command: {command}", exception))
            .WithInbound(new PermissionMiddleware())
            .WithInbound(new TokenBucketMiddleware())
            .WithInbound(new ConcurrencyMiddleware())
            .WithInbound(new RateLimitMiddleware())
            .WithInbound(new UnwrapPacketMiddleware())
            .WithInbound(new TimeoutMiddleware())
            .WithOutbound(new WrapPacketMiddleware())
            .WithHandler(() => new HandshakeOps())
            .WithHandler(() => new AccountOps(credentials))
            .WithHandler(() => new PasswordOps(credentials))
        );

        channel.Activate();
        HostProtocol protocol = new(channel);

        Listener = new HostListener(protocol);
    }

    /// <summary>
    /// Verifies that the catalog contains the expected deserializer, and that
    /// serialization produces the same magic number we query for.
    /// </summary>
    private static void SelfTestPackets()
    {
        var log = InstanceManager.Instance.GetOrCreateInstance<ILogger>();
        var catalog = InstanceManager.Instance.GetExistingInstance<IPacketCatalog>();
        if (catalog is null)
        {
            log.Error("[SELFTEST] IPacketCatalog is NULL. Registry.Load() probably didn't run.");
            return;
        }

        // A) Does catalog have Credentials deserializer?
        var credMagic = PacketMagic.CREDENTIALS.AsUInt32();
        Boolean has = catalog.TryGetDeserializer(credMagic, out PacketDeserializer? des);
        log.Info("[SELFTEST] Catalog#{0} has Creds deserializer? {1} (magic=0x{2:X8})",
                 catalog.GetHashCode(), has, credMagic);

        // B) Serialize a dummy CredentialsPacket and confirm 4 bytes header == credMagic
        var pkt = new CredentialsPacket(); // Username/Password default empty
        var buf = pkt.Serialize();

        if (buf.Length < 4)
        {
            log.Error("[SELFTEST] CredentialsPacket serialize length < 4!");
            return;
        }

        UInt32 magicLE = buf.ReadMagicNumberLE();
        log.Info("[SELFTEST] CredentialsPacket TX magicLE=0x{0:X8} len={1}", magicLE, buf.Length);

        if (magicLE != credMagic)
        {
            log.Error("[SELFTEST] MAGIC MISMATCH! enum=0x{0:X8}, serialized=0x{1:X8}", credMagic, magicLE);
        }

        // C) TryDeserialize with the same catalog
        if (catalog.TryDeserialize(buf, out IPacket? parsed))
        {
            log.Info("[SELFTEST] TryDeserialize OK -> {0}", parsed.GetType().Name);
        }
        else
        {
            log.Warn("[SELFTEST] TryDeserialize FAIL for CredentialsPacket buffer (magic=0x{0:X8})", magicLE);
        }
    }
}