using Nalix.Game.Domain.Entities;
using Nalix.Shared.Time;

namespace Nalix.Game.Domain.Models.Items;

/// <summary>
/// Đại diện cho một bản đồ vật phẩm trong trò chơi.
/// </summary>
public sealed class ItemMap
{
    /// <summary>
    /// ID duy nhất của vật phẩm trên bản đồ.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// ID của người chơi sở hữu vật phẩm.
    /// </summary>
    public int PlayerId { get; set; }

    /// <summary>
    /// Bán kính tương tác của vật phẩm.
    /// </summary>
    public byte Radius { get; set; }

    /// <summary>
    /// Thời gian còn lại trước khi vật phẩm biến mất.
    /// </summary>
    public long LeftTime { get; set; }

    /// <summary>
    /// Kiểm tra xem vật phẩm đã hết hạn hay chưa.
    /// </summary>
    public bool IsExpired => Clock.UnixMillisecondsNow() > LeftTime;

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