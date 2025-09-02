using Nalix.Common.Packets.Abstractions;
using Nalix.Shared.Injection;
using Nalix.Shared.Messaging.Catalog;
using System.Diagnostics.CodeAnalysis;

namespace Nalix.Desktop;

/// <summary>
/// Performs client-side initialization such as packet registrations
/// and service wiring using the existing InstanceManager.
/// </summary>
internal static class Registry
{
    /// <summary>
    /// Initializes client components. Call this once at startup.
    /// </summary>
    [RequiresUnreferencedCode("Calls Nalix.Shared.Messaging.Catalog.PacketCatalogFactory.CreateCatalog()")]
    public static void Load()
    {
        // Register logger first so other components can use it.
#if DEBUG
        InstanceManager.Instance.Register<Common.Logging.Abstractions.ILogger>(Logging.NLogix.Host.Instance);
#else
        Logging.Extensions.NLogixFx.MinimumLevel = (Common.Logging.Models.LogLevel)255;
#endif



        // 1) Build packet catalog.
        var factory = new PacketCatalogFactory();

        // Register packets here (single source of truth).
        //_ = factory.RegisterPacket<CredentialsPacket>();
        // _ = factory.RegisterPacket<PasswordChangePacket>();
        // _ = factory.RegisterPacket<...>();

        IPacketCatalog catalog = factory.CreateCatalog();

        // 2) Expose catalog through your current service locator.
        InstanceManager.Instance.Register(catalog);

        // 3) (Optional) Add more client init steps here later:
        // - Preload assets
        // - Warm up caches
        // - Initialize UI/Scenes
    }
}
