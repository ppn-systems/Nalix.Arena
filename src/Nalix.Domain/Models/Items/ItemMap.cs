using Nalix.Domain.Entities.Player;
using Nalix.Framework.Time;

namespace Nalix.Domain.Models.Items;

/// <summary>
/// Đại diện cho một bản đồ vật phẩm trong trò chơi.
/// </summary>
public sealed class ItemMap
{
    /// <summary>
    /// ID duy nhất của vật phẩm trên bản đồ.
    /// </summary>
    public System.Int32 Id { get; set; }

    /// <summary>
    /// ID của người chơi sở hữu vật phẩm.
    /// </summary>
    public System.Int32 PlayerId { get; set; }

    /// <summary>
    /// Bán kính tương tác của vật phẩm.
    /// </summary>
    public System.Byte Radius { get; set; }

    /// <summary>
    /// Thời gian còn lại trước khi vật phẩm biến mất.
    /// </summary>
    public System.Int64 LeftTime { get; set; }

    /// <summary>
    /// Kiểm tra xem vật phẩm đã hết hạn hay chưa.
    /// </summary>
    public System.Boolean IsExpired => Clock.UnixMillisecondsNow() > LeftTime;

    /// <summary>
    /// Thông tin của vật phẩm.
    /// </summary>
    public Item Item { get; set; }

    /// <summary>
    /// Vị trí hiện tại của vật phẩm trên bản đồ.
    /// </summary>
    public Position Position { get; set; }

    /// <summary>
    /// Nguồn gốc của vật phẩm khi xuất hiện.
    /// </summary>
    public ItemDropOrigin DropOrigin { get; set; } = ItemDropOrigin.None;
}