using Nalix.Game.Domain.Models.Characters;
using Nalix.Game.Domain.Models.Items;
using Nalix.Game.Domain.Models.Monsters;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Nalix.Game.Domain.Models.Maps.Zones;

public class Zone : IZone
{
    public int Id { get; set; }                          // ID duy nhất của Zone
    public int MapId { get; set; }                       // ID bản đồ mà zone này thuộc về
    public string Name { get; set; }                     // Tên khu vực

    public IMap Map { get; set; }
    public List<IMonster> Monsters { get; set; }         // Danh sách quái vật trong zone
    public ConcurrentDictionary<int, ItemMap> ItemMaps { get; set; }
    public ConcurrentDictionary<int, Character> Characters { get; set; }         // Danh sách người chơi trong zone

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "<Pending>")]
    public Zone(int id, IMap map)
    {
        Id = id;
        Map = map;
    }
}