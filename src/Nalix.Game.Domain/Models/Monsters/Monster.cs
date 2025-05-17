using Nalix.Game.Domain.Entities;
using Nalix.Shared.Time;
using System.Collections.Generic;

namespace Nalix.Game.Domain.Models.Monsters;

public abstract class Monster : IMonster
{
    public int Id { get; set; }
    public int IdMap { get; set; }
    public string Name { get; set; }
    public long TimeAttack { get; set; }
    public bool IsAlive => Stats.Health > 0;

    public LootTable Loot { get; set; }                   // Loot table của quái vật
    public MonsterType Type { get; set; }                 // Loại quái vật
    public Position Position { get; set; }
    public MonsterStats Stats { get; set; }              // Các chỉ số quái vật
    public RefreshInfo Refresh { get; set; }              // Thông tin làm mới quái vật
    public Dictionary<int, int> PlayerAttack { get; set; }

    public Monster()
    {
        // Khởi tạo các giá trị mặc định nếu cần
        Id = -1;
        IdMap = -1;
        TimeAttack = 10000 + Clock.UnixMillisecondsNow();

        Position = new Position(0, 0);
        Stats = new MonsterStats();
        Loot = new LootTable();
        Refresh = new RefreshInfo();
        Type = MonsterType.Melee;
    }
}