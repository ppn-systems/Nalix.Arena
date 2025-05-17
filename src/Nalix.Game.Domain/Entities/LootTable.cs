using System.Collections.Generic;

namespace Nalix.Game.Domain.Entities;

/// <summary>
/// Lớp đại diện cho bảng vật phẩm rơi (loot table) trong game.
/// </summary>
public class LootTable
{
    /// <summary>
    /// Danh sách các mục vật phẩm có thể rơi từ bảng này.
    /// </summary>
    public List<LootEntry> Entries { get; set; } = [];
}