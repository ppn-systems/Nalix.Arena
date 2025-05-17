namespace Nalix.Game.Domain.Entities;

public class LootEntry
{
    public int ItemId { get; set; }              // ID vật phẩm
    public float DropChance { get; set; }       // Xác suất rơi (0.0 - 1.0)
}