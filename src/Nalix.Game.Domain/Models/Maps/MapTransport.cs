namespace Nalix.Game.Domain.Models.Maps;

internal class MapTransport
{
    /// <summary>
    /// Gets or sets the identifier for the source map.
    /// </summary>
    public short SourceMapId { get; set; }

    /// <summary>
    /// Gets or sets the cost of using the transport (e.g., in-game currency).
    /// </summary>
    public int Cost { get; set; }

    /// <summary>
    /// Gets or sets the name of the transport location or destination map.
    /// </summary>
    public string TransportName { get; set; }
}