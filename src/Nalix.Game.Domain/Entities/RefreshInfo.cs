namespace Nalix.Domain.Entities;

/// <summary>
/// Lớp đại diện cho thông tin làm mới (refresh) của một đối tượng trong game.
/// </summary>
/// <remarks>
/// Constructor with parameters.
/// </remarks>
public sealed class RefreshInfo(System.Boolean isRefresh, System.Int64 timeRefresh)
{
    /// <summary>
    /// Trạng thái cho biết đối tượng có được làm mới hay không.
    /// </summary>
    public System.Boolean IsRefresh { get; set; } = isRefresh;

    /// <summary>
    /// Thời điểm làm mới tiếp theo của đối tượng (tính bằng Unix milliseconds).
    /// </summary>
    public System.Int64 TimeRefresh { get; set; } = timeRefresh;

    /// <summary>
    /// Parameterless constructor.
    /// </summary>
    public RefreshInfo() : this(true, 5000)
    {
    }
}