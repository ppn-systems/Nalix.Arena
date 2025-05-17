using Nalix.Game.Domain.Models.Characters;
using Nalix.Game.Domain.Models.Items;
using Nalix.Game.Domain.Models.Maps;
using Nalix.Game.Domain.Models.Monsters;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Nalix.Game.Domain.Models.Zones;

/// <summary>
/// Đại diện cho một khu vực trong bản đồ trò chơi.
/// </summary>
public interface IZone
{
    /// <summary>
    /// ID duy nhất của khu vực.
    /// </summary>
    uint Id { get; }

    /// <summary>
    /// Bản đồ mà khu vực thuộc về.
    /// </summary>
    IMap Map { get; }

    /// <summary>
    /// Danh sách các quái vật hiện có trong khu vực.
    /// </summary>
    List<IMonster> Monsters { get; }

    /// <summary>
    /// Từ điển chứa các vật phẩm có trên bản đồ trong khu vực.
    /// </summary>
    ConcurrentDictionary<int, ItemMap> ItemMaps { get; }

    /// <summary>
    /// Từ điển chứa các nhân vật hiện diện trong khu vực.
    /// </summary>
    ConcurrentDictionary<int, Character> Characters { get; }
}