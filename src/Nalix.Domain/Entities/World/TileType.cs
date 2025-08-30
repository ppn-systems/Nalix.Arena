namespace Nalix.Domain.Entities.World;

/// <summary>
/// Danh mục các loại ô (tile) trên bản đồ trong game.
/// </summary>
public enum TileType : System.UInt16
{
    /// <summary>
    /// Không có loại ô (mặc định hoặc trống).
    /// </summary>
    None = 0,

    /// <summary>
    /// Ô đất, có thể di chuyển qua.
    /// </summary>
    Ground = 1,

    /// <summary>
    /// Ô tường, chặn di chuyển.
    /// </summary>
    Wall = 2,

    /// <summary>
    /// Ô nước, có thể ảnh hưởng đến di chuyển hoặc gây hiệu ứng.
    /// </summary>
    Water = 3,

    /// <summary>
    /// Ô dịch chuyển, cho phép di chuyển đến vị trí khác.
    /// </summary>
    Teleport = 4
}