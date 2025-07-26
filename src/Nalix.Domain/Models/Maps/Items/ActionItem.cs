namespace Nalix.Domain.Models.Maps.Items;

/// <summary>
/// Đại diện cho một cặp khóa-giá trị để xác định hành động trong bản đồ trò chơi.
/// </summary>
public class ActionItem
{
    /// <summary>
    /// Lấy hoặc đặt khóa xác định loại hành động.
    /// </summary>
    public System.Int16 Key { get; set; }

    /// <summary>
    /// Lấy hoặc đặt giá trị mô tả hoặc thông tin liên quan đến hành động.
    /// </summary>
    public System.Int32 Value { get; set; }
}