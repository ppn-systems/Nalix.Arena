namespace Nalix.Game.Domain.Entities;

/// <summary>
/// Lớp đại diện cho một mục vật phẩm trong bảng vật phẩm rơi (loot table) của game.
/// </summary>
public class LootEntry
{
    /// <summary>
    /// Mã định danh (ID) của vật phẩm.
    /// </summary>
    public System.UInt32 ItemId { get; set; }

    /// <summary>
    /// Xác suất rơi của vật phẩm, nằm trong khoảng từ 0.0 đến 1.0.
    /// </summary>
    public System.Single DropChance { get; set; }
}