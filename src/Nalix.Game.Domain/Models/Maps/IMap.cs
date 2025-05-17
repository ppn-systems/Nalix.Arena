using Nalix.Game.Domain.Models.Maps.Items;
using Nalix.Game.Domain.Models.Maps.NPCs;
using Nalix.Game.Domain.Models.Maps.Zones;
using Nalix.Game.Domain.Models.Monsters;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nalix.Game.Domain.Models.Maps;

public interface IMap
{
    int Id { get; }
    string Name { get; set; }
    long TimeMap { get; set; }
    byte ZoneCount { get; set; }
    bool IsRunning { get; set; }
    byte MaxPlayers { get; set; }

    TileMap TileMap { get; }
    List<Zone> Zones { get; }
    Task HandleZone { get; set; }
    List<Npc> Npcs { get; set; }
    List<Monster> Monsters { get; set; }
    List<WayPoint> WayPoints { get; set; }
    List<ActionItem> ActionItems { get; set; }
    List<BackgroundItem> BackgroundItems { get; set; }

    void SetZone();

    Zone GetZoneNotMaxPlayer();

    Zone GetZonePlayer();
}