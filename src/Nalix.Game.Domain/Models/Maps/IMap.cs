using Nalix.Game.Domain.Models.Maps.Zones;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nalix.Game.Domain.Models.Maps;

public interface IMap
{
    int Id { get; }
    List<Zone> Zones { get; }
    TileMap TileMap { get; }
    long TimeMap { get; set; }
    bool IsRunning { get; set; }
    bool IsStop { get; set; }
    Task HandleZone { get; set; }

    void SetZone();

    Zone GetZoneNotMaxPlayer();

    Zone GetZonePlayer();
}