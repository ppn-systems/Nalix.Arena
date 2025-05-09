using Nalix.Game.Domain.Shared;
using Nalix.Shared.Time;
using System.Collections.Generic;

namespace Nalix.Game.Domain.Models.Monsters;

public abstract class Monster : IMonster
{
    public int Id { get; set; }
    public int IdMap { get; set; }
    public Position Position { get; set; }
    public string Name { get; set; }

    public bool IsAlive => Health > 0;
    public int Level { get; set; }
    public int Armor { get; set; }
    public int Damage { get; set; }
    public int Health { get; set; }
    public int MaxHealth { get; set; }
    public int Experience { get; set; }
    public long TimeAttack { get; set; }

    public int[] LootTable { get; set; } = [];
    public float LootDropChance { get; set; } = 0.1f;

    public bool IsRefresh { get; set; }
    public long TimeRefresh { get; set; }

    public MonsterType Type { get; set; }

    public Dictionary<int, int> SessionAttack { get; set; }

    public Monster()
    {
        // Khởi tạo các giá trị mặc định nếu cần
        Id = -1;
        IdMap = -1;
        Position = new Position(0, 0);

        TimeRefresh = 0;
        IsRefresh = false;

        Name = string.Empty;
        Level = 1;
        Armor = 0;
        Damage = 0;
        Health = 100;
        MaxHealth = 100;
        Experience = 0;
        TimeAttack = 10000 + Clock.UnixMillisecondsNow(); // Thời gian tấn công mặc định (ms)
    }
}