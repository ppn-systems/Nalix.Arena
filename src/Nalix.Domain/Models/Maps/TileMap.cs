using Nalix.Domain.Entities.World;

namespace Nalix.Domain.Models.Maps;

/// <summary>
/// Lớp đại diện cho một bản đồ ô (tile map) trong game, chứa thông tin về kích thước, ô và nền.
/// </summary>
public sealed class TileMap
{
    /// <summary>
    /// Mã định danh (ID) của hình nền của bản đồ.
    /// </summary>
    public System.UInt16 BackgroundId { get; set; }

    /// <summary>
    /// Loại hình nền của bản đồ.
    /// </summary>
    public System.Byte BackgroundType { get; set; }

    /// <summary>
    /// Độ dịch chuyển theo trục X của bản đồ (tính bằng pixel).
    /// </summary>
    public System.Int16 OffsetX { get; set; }

    /// <summary>
    /// Độ dịch chuyển theo trục Y của bản đồ (tính bằng pixel).
    /// </summary>
    public System.Int16 OffsetY { get; set; }

    /// <summary>
    /// Chiều rộng của bản đồ tính bằng số ô (tiles).
    /// </summary>
    public System.UInt16 WidthInTiles { get; set; }

    /// <summary>
    /// Chiều cao của bản đồ tính bằng số ô (tiles).
    /// </summary>
    public System.UInt16 HeightInTiles { get; set; }

    /// <summary>
    /// Chiều rộng của bản đồ tính bằng pixel (mỗi ô có kích thước 32 pixel).
    /// </summary>
    public System.Int32 WidthInPixels => WidthInTiles * 32;

    /// <summary>
    /// Chiều cao của bản đồ tính bằng pixel (mỗi ô có kích thước 32 pixel).
    /// </summary>
    public System.Int32 HeightInPixels => HeightInTiles * 32;

    /// <summary>
    /// Mảng chứa mã định danh (ID) của các ô trên bản đồ.
    /// </summary>
    public System.UInt16[] TileIds { get; set; }

    /// <summary>
    /// Mảng chứa loại (type) của các ô trên bản đồ.
    /// </summary>
    public TileType[] TileTypes { get; set; }
}