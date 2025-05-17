namespace Nalix.Game.Domain.Entities;

/// <summary>
/// Đại diện cho một vị trí 2D trên bản đồ trò chơi với tọa độ X và Y.
/// </summary>
/// <remarks>
/// Khởi tạo một vị trí mới với tọa độ X và Y được chỉ định.
/// </remarks>
/// <param name="x">Tọa độ X của vị trí.</param>
/// <param name="y">Tọa độ Y của vị trí.</param>
/// <example>
/// Ví dụ: <c>new Position(10, 20)</c> tạo một vị trí tại (10, 20).
/// </example>
public struct Position(short x, short y)
{
    /// <summary>
    /// Lấy hoặc đặt tọa độ X của vị trí.
    /// </summary>
    /// <example>Ví dụ: 10 cho vị trí ngang trên bản đồ.</example>
    public short X { get; set; } = x;

    /// <summary>
    /// Lấy hoặc đặt tọa độ Y của vị trí.
    /// </summary>
    /// <example>Ví dụ: 20 cho vị trí dọc trên bản đồ.</example>
    public short Y { get; set; } = y;

    /// <summary>
    /// Tính khoảng cách Euclidean đến một vị trí khác.
    /// </summary>
    /// <param name="other">Vị trí khác để tính khoảng cách.</param>
    /// <returns>Khoảng cách Euclidean giữa vị trí hiện tại và vị trí <paramref name="other"/>.</returns>
    /// <example>
    /// Ví dụ: Nếu vị trí hiện tại là (0, 0) và <paramref name="other"/> là (3, 4), hàm trả về 5.0.
    /// </example>
    public readonly double DistanceTo(Position other)
    {
        int dx = X - other.X;
        int dy = Y - other.Y;
        return System.Math.Sqrt(dx * dx + dy * dy);
    }
}