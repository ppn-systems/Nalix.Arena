namespace Nalix.Game.Domain.Models.Maps;

public sealed class TileMap
{
    public ushort WidthInTiles { get; set; }
    public ushort HeightInTiles { get; set; }

    public int WidthInPixels => WidthInTiles * 32;
    public int HeightInPixels => HeightInTiles * 32;

    public ushort[] TileIds { get; set; }
    public ushort[] TileTypes { get; set; }

    public ushort BackgroundId { get; set; }
    public byte BackgroundType { get; set; }

    public short OffsetX { get; set; }
    public short OffsetY { get; set; }
}