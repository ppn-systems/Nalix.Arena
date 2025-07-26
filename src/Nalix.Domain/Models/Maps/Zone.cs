using Nalix.Domain.Interface;
using Nalix.Domain.Models.Characters;
using Nalix.Domain.Models.Items;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Nalix.Domain.Models.Maps;

public sealed class Zone : IZone
{
    /// <inheritdoc />
    public System.UInt32 Id { get; set; }

    /// <inheritdoc />
    public IMap Map { get; set; }

    /// <inheritdoc />
    public List<IMonster> Monsters { get; set; }

    /// <inheritdoc />
    public ConcurrentDictionary<System.Int32, ItemMap> ItemMaps { get; set; }

    /// <inheritdoc />
    public ConcurrentDictionary<System.Int32, Character> Characters { get; set; }

    /// <summary>
    /// Khởi tạo một khu vực mới với ID và bản đồ được chỉ định.
    /// </summary>
    /// <param name="id">ID của khu vực.</param>
    /// <param name="map">Bản đồ mà khu vực thuộc về.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "<Pending>")]
    public Zone(System.UInt32 id, IMap map)
    {
        Id = id;
        Map = map;
    }
}