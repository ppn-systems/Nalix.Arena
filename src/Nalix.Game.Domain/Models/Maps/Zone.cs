using Nalix.Game.Domain.Models.Characters;
using Nalix.Game.Domain.Models.Monsters;
using System.Collections.Generic;

namespace Nalix.Game.Domain.Models.Maps;

public class Zone
{
    public int Id { get; set; }                          // ID duy nhất của Zone
    public int MapId { get; set; }                       // ID bản đồ mà zone này thuộc về
    public string Name { get; set; }                     // Tên khu vực
    public List<Player> Players { get; set; }         // Danh sách người chơi trong zone
    public List<IMonster> Monsters { get; set; }         // Danh sách quái vật trong zone
}