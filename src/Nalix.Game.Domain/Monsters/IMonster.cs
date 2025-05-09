namespace Nalix.Game.Domain.Monsters;

public interface IMonster
{
    int Id { get; set; }
    int IdMap { get; set; }
    short X { get; set; }
    short Y { get; set; }
    string Name { get; set; }

    bool IsAlive { get; }
    int Level { get; set; }                   // Cấp độ của quái vật
    int Armor { get; set; }                   // Chỉ số phòng thủ của quái vật
    int Damage { get; set; }
    int Health { get; set; }
    int MaxHealth { get; set; }
    int Experience { get; set; }
    int AttackSpeed { get; set; }

    int[] LootTable { get; set; } // Danh sách ID của loot table
    float LootDropChance { get; set; } // Tỷ lệ rơi vật phẩm

    bool IsRefresh { get; set; }
    long TimeRefresh { get; set; }

    MonsterType Type { get; set; }

    // Method suggestions
    void TakeDamage(int amount);             // Quái vật nhận sát thương

    void Attack(IMonster target);            // Quái vật tấn công đối tượng khác

    void Respawn();                          // Quái vật sống lại sau khi chết

    void Die();                              // Quái vật chết

    void Heal(int amount);                   // Hồi phục máu cho quái vật

    void DropLoot();                         // Quái vật rơi vật phẩm

    void MoveTo(short x, short y);           // Di chuyển quái vật tới vị trí mới
}