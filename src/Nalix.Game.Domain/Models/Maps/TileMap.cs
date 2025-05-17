using Nalix.Game.Domain.Entities;

namespace Nalix.Game.Domain.Models.Maps;

/// <summary>
/// Lớp đại diện cho một bản đồ ô (tile map) trong game, chứa thông tin về kích thước, ô và nền.
/// </summary>
public sealed class TileMap
{
    /// <summary>
    /// Mã định danh (ID) của hình nền của bản đồ.
    /// </summary>
    public ushort BackgroundId { get; set; }

    /// <summary>
    /// Loại hình nền của bản đồ.
    /// </summary>
    public byte BackgroundType { get; set; }

    /// <summary>
    /// Độ dịch chuyển theo trục X của bản đồ (tính bằng pixel).
    /// </summary>
    public short OffsetX { get; set; }

    /// <summary>
    /// Độ dịch chuyển theo trục Y của bản đồ (tính bằng pixel).
    /// </summary>
    public short OffsetY { get; set; }

    /// <summary>
    /// Chiều rộng của bản đồ tính bằng số ô (tiles).
    /// </summary>
    public ushort WidthInTiles { get; set; }

    /// <summary>
    /// Chiều cao của bản đồ tính bằng số ô (tiles).
    /// </summary>
    public ushort HeightInTiles { get; set; }

    /// <summary>
    /// Chiều rộng của bản đồ tính bằng pixel (mỗi ô có kích thước 32 pixel).
    /// </summary>
    public int WidthInPixels => WidthInTiles * 32;

    /// <summary>
    /// Chiều cao của bản đồ tính bằng pixel (mỗi ô có kích thước 32 pixel).
    /// </summary>
    public int HeightInPixels => HeightInTiles * 32;

    /// <summary>
    /// Mảng chứa mã định danh (ID) của các ô trên bản đồ.
    /// </summary>
    public ushort[] TileIds { get; set; }

    /// <summary>
    /// Mảng chứa loại (type) của các ô trên bản đồ.
    /// </summary>
    public TileType[] TileTypes { get; set; }
}