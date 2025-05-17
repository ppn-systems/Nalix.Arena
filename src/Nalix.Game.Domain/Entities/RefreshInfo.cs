namespace Nalix.Game.Domain.Entities;

/// <summary>
/// Lớp đại diện cho thông tin làm mới (refresh) của một đối tượng trong game.
/// </summary>
public sealed class RefreshInfo
{
    /// <summary>
    /// Trạng thái cho biết đối tượng có được làm mới hay không.
    /// </summary>
    public bool IsRefresh { get; set; }

    /// <summary>
    /// Thời điểm làm mới tiếp theo của đối tượng (tính bằng Unix milliseconds).
    /// </summary>
    public long TimeRefresh { get; set; }
}