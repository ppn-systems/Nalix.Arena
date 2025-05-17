using Nalix.Game.Domain.Models.Maps.Items;
using Nalix.Game.Domain.Models.Maps.NPCs;
using Nalix.Game.Domain.Models.Maps.Zones;
using Nalix.Game.Domain.Models.Monsters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nalix.Game.Domain.Models.Maps;

public class Map : IMap
{
    public int Id { get; set; }
    public string Name { get; set; }
    public long TimeMap { get; set; }
    public bool IsRunning { get; set; }
    public byte ZoneCount { get; set; }
    public byte MaxPlayers { get; set; }

    public Task HandleZone { get; set; }
    public TileMap TileMap { get; set; }
    public List<Zone> Zones { get; set; }
    public List<Npc> Npcs { get; set; }
    public List<Monster> Monsters { get; set; }
    public List<WayPoint> WayPoints { get; set; }
    public List<ActionItem> ActionItems { get; set; }
    public List<BackgroundItem> BackgroundItems { get; set; }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "<Pending>")]
    public Map(int id, TileMap tileMap)
    {
        Id = id;
        Zones = [];
        TimeMap = -1;
        IsRunning = false;
        TileMap = tileMap ?? throw new ArgumentNullException(nameof(tileMap));
    }

    public void SetZone()
    {
        for (var i = 0; i < ZoneCount; i++)
        {
            Zones.Add(new Zone(i, this));
        }
    }

    /// <summary>
    /// Cập nhật trạng thái của bản đồ.
    /// </summary>
    /// <param name="time"></param>
    public virtual void Update(long time)
    {
    }

    public Zone GetZoneNotMaxPlayer()
        => Zones.FirstOrDefault(x => x.Characters.Count < MaxPlayers);

    public Zone GetZonePlayer() => Zones.FirstOrDefault(x => !x.Characters.IsEmpty);
}