namespace Nalix.Game.Domain.Models.Monsters;

public class LootTable
{
    public int[] LootIds { get; set; }          // Danh sách ID của loot table
    public float LootDropChance { get; set; }   // Tỷ lệ rơi vật phẩm
}