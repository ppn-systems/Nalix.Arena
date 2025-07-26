using Nalix.Domain.Models.Characters;
using Nalix.Domain.Models.Items;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Nalix.Domain.Interface;

/// <summary>
/// Đại diện cho một khu vực trong bản đồ trò chơi.
/// </summary>
public interface IZone
{
    /// <summary>
    /// ID duy nhất của khu vực.
    /// </summary>
    System.UInt32 Id { get; }

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
    ConcurrentDictionary<System.Int32, ItemMap> ItemMaps { get; }

    /// <summary>
    /// Từ điển chứa các nhân vật hiện diện trong khu vực.
    /// </summary>
    ConcurrentDictionary<System.Int32, Character> Characters { get; }
}