using Nalix.Game.Domain.Entities;

namespace Nalix.Game.Domain.Models.Monsters;

public interface IMonster
{
    int Id { get; set; }
    int IdMap { get; set; }
    string Name { get; set; }
    bool IsAlive { get; }
    long TimeAttack { get; set; }

    Position Position { get; set; }
    MonsterStats Stats { get; set; }              // Các chỉ số quái vật
    LootTable Loot { get; set; }                   // Loot table của quái vật
    RefreshInfo Refresh { get; set; }              // Thông tin làm mới quái vật
    MonsterType Type { get; set; }                 // Loại quái vật
}