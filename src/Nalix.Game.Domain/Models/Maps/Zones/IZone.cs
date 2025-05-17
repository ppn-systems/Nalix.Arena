using Nalix.Game.Domain.Models.Characters;
using Nalix.Game.Domain.Models.Items;
using Nalix.Game.Domain.Models.Monsters;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Nalix.Game.Domain.Models.Maps.Zones;

public interface IZone
{
    int Id { get; }
    IMap Map { get; }

    List<IMonster> Monsters { get; }
    ConcurrentDictionary<int, ItemMap> ItemMaps { get; }
    ConcurrentDictionary<int, Character> Characters { get; }
}