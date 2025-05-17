using Nalix.Game.Domain.Entities;

namespace Nalix.Game.Domain.Models.Maps.Items;

/// <summary>
/// Lớp đại diện cho một vật phẩm nền (background item) trên bản đồ trong game.
/// </summary>
public class BackgroundItem
{
    /// <summary>
    /// Mã định danh của vật phẩm nền.
    /// </summary>
    public short ItemId { get; set; }

    /// <summary>
    /// Vị trí của vật phẩm nền trên bản đồ.
    /// </summary>
    public Position Position { get; set; }
}