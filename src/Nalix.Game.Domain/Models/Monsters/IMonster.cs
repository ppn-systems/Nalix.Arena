using Nalix.Game.Domain.Shared;

namespace Nalix.Game.Domain.Models.Monsters;

public interface IMonster
{
    int Id { get; set; }
    int IdMap { get; set; }
    public Position Position { get; set; }
    string Name { get; set; }

    bool IsAlive { get; }
    int Level { get; set; }                   // Cấp độ của quái vật
    int Armor { get; set; }                   // Chỉ số phòng thủ của quái vật
    int Damage { get; set; }
    int Health { get; set; }
    int MaxHealth { get; set; }
    int Experience { get; set; }
    long TimeAttack { get; set; }

    int[] LootTable { get; set; } // Danh sách ID của loot table
    float LootDropChance { get; set; } // Tỷ lệ rơi vật phẩm

    bool IsRefresh { get; set; }
    long TimeRefresh { get; set; }

    MonsterType Type { get; set; }
}