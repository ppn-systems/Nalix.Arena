using System.Collections.Generic;

namespace Nalix.Game.Domain.Entities;

public class LootTable
{
    // Danh sách vật phẩm có thể rơi
    public List<LootEntry> Entries { get; set; } = [];
}