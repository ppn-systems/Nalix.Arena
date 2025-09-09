using Nalix.Common.Packets.Abstractions;
using Nalix.Communication.Collections;
using Nalix.Framework.Injection;
using Nalix.Shared.Messaging.Catalog;

namespace Nalix.Communication;

/// <summary>
/// Performs client-side initialization such as packet registrations
/// and service wiring using the existing InstanceManager.
/// </summary>
public static class Registry
{
    /// <summary>
    /// Initializes client components. Call this once at startup.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode(
        "Calls Nalix.Shared.Messaging.Catalog.PacketCatalogFactory.CreateCatalog()")]
    public static void Load()
    {
        // 1) Build packet catalog.
        var factory = new PacketCatalogFactory();

        // REGISTER packets here (single source of truth).
        _ = factory.RegisterPacket<ResponsePacket>();
        _ = factory.RegisterPacket<CredentialsPacket>();
        _ = factory.RegisterPacket<CredsUpdatePacket>();

        IPacketCatalog catalog = factory.CreateCatalog();

        // 2) Expose catalog through your current service locator.
        InstanceManager.Instance.Register<IPacketCatalog>(catalog);

        // 3) (Optional) Add more client init steps here later:
        // - Preload assets
        // - Warm up caches
        // - Initialize UI/Scenes
    }
}
