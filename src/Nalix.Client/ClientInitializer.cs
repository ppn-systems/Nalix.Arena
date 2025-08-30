using Nalix.Common.Logging.Abstractions;
using Nalix.Common.Packets.Abstractions;
using Nalix.Logging;
using Nalix.Shared.Injection;
using Nalix.Shared.Messaging.Catalog;

namespace Nalix.Client;

/// <summary>
/// Performs client-side initialization such as packet registrations
/// and service wiring using the existing InstanceManager.
/// </summary>
internal static class ClientInitializer
{
    /// <summary>
    /// Initializes client components. Call this once at startup.
    /// </summary>
    public static void Load()
    {
        // Register logger first so other components can use it.
#if DEBUG
        InstanceManager.Instance.Register<ILogger>(NLogix.Host.Instance);
#endif

        // 1) Build packet catalog.
        var factory = new PacketCatalogFactory();

        // Register packets here (single source of truth).
        //_ = factory.RegisterPacket<CredentialsPacket>();
        // _ = factory.RegisterPacket<PasswordChangePacket>();
        // _ = factory.RegisterPacket<...>();

        IPacketCatalog catalog = factory.CreateCatalog();

        // 2) Expose catalog through your current service locator.
        InstanceManager.Instance.Register<IPacketCatalog>(catalog);

        // 3) (Optional) Add more client init steps here later:
        // - Preload assets
        // - Warm up caches
        // - Initialize UI/Scenes
    }
}
