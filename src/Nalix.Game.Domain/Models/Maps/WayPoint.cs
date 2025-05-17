namespace Nalix.Game.Domain.Models.Maps;

/// <summary>
/// Đại diện cho một điểm waypoint trong bản đồ trò chơi.
/// </summary>
public sealed class WayPoint
{
    /// <summary>
    /// Tọa độ X nhỏ nhất của waypoint (biên trái).
    /// </summary>
    public short MinX { get; set; }

    /// <summary>
    /// Tọa độ Y nhỏ nhất của waypoint (biên trên).
    /// </summary>
    public short MinY { get; set; }

    /// <summary>
    /// Tọa độ X lớn nhất của waypoint (biên phải).
    /// </summary>
    public short MaxX { get; set; }

    /// <summary>
    /// Tọa độ Y lớn nhất của waypoint (biên dưới).
    /// </summary>
    public short MaxY { get; set; }

    /// <summary>
    /// Xác định liệu waypoint có thể được đi vào hay không.
    /// </summary>
    public bool IsEnterable { get; set; }

    /// <summary>
    /// Tên của waypoint, dùng trong giao diện trò chơi hoặc để định danh.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// ID bản đồ tiếp theo khi đi qua waypoint này (có thể là teleport).
    /// </summary>
    public short NextMapId { get; set; }

    /// <summary>
    /// Khởi tạo các giá trị mặc định cho waypoint.
    /// </summary>
    public WayPoint()
    {
        NextMapId = -1;
        Name = string.Empty;
        IsEnterable = true;
    }
}