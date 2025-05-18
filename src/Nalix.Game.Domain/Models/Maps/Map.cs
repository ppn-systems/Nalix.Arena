using Nalix.Game.Domain.Common;
using Nalix.Game.Domain.Interface;
using Nalix.Game.Domain.Models.Maps.Items;
using Nalix.Game.Domain.Models.Monsters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nalix.Game.Domain.Models.Maps;

/// <summary>
/// Lớp đại diện cho một bản đồ trong game, triển khai giao diện <see cref="IMap"/>.
/// </summary>
public class Map : NamedEntity<uint>, IMap
{
    /// <summary>
    /// Thời gian liên quan đến bản đồ (có thể là thời gian tạo hoặc cập nhật, tính bằng Unix milliseconds).
    /// </summary>
    public long TimeMap { get; set; }

    /// <summary>
    /// Trạng thái hoạt động của bản đồ (đang chạy hay không).
    /// </summary>
    public bool IsRunning { get; set; }

    /// <summary>
    /// Số lượng vùng (zone) trong bản đồ.
    /// </summary>
    public byte ZoneCount { get; set; }

    /// <summary>
    /// Số lượng người chơi tối đa được phép trên bản đồ.
    /// </summary>
    public byte MaxPlayers { get; set; }

    /// <summary>
    /// Tác vụ xử lý các vùng trong bản đồ.
    /// </summary>
    public Task HandleZone { get; set; }

    /// <summary>
    /// Bản đồ ô (tile map) chứa thông tin về cấu trúc ô của bản đồ.
    /// </summary>
    public TileMap TileMap { get; set; }

    /// <summary>
    /// Danh sách các NPC (nhân vật không chơi) trên bản đồ.
    /// </summary>
    public List<Npc> Npcs { get; set; }

    /// <summary>
    /// Danh sách các vùng (zone) trong bản đồ.
    /// </summary>
    public List<Zone> Zones { get; set; }

    /// <summary>
    /// Danh sách các quái vật trên bản đồ.
    /// </summary>
    public List<Monster> Monsters { get; set; }

    /// <summary>
    /// Danh sách các điểm cách (waypoint) trên bản đồ.
    /// </summary>
    public List<WayPoint> WayPoints { get; set; }

    /// <summary>
    /// Danh sách các vật phẩm hành động (action items) trên bản đồ.
    /// </summary>
    public List<ActionItem> ActionItems { get; set; }

    /// <summary>
    /// Danh sách các vật phẩm nền (background items) trên bản đồ.
    /// </summary>
    public List<BackgroundItem> BackgroundItems { get; set; }

    /// <summary>
    /// Khởi tạo một bản đồ mới với mã định danh và bản đồ ô.
    /// </summary>
    /// <param name="id">Mã định danh của bản đồ.</param>
    /// <param name="tileMap">Bản đồ ô (tile map) của bản đồ.</param>
    /// <exception cref="ArgumentNullException">Ném ra nếu <paramref name="tileMap"/> là null.</exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "<Pending>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "<Pending>")]
    public Map(uint id, TileMap tileMap)
    {
        Id = id;
        Zones = [];
        TimeMap = -1;
        IsRunning = false;
        TileMap = tileMap ?? throw new ArgumentNullException(nameof(tileMap));
    }

    /// <summary>
    /// Thiết lập các vùng (zone) cho bản đồ bằng cách tạo các vùng mới dựa trên <see cref="ZoneCount"/>.
    /// </summary>
    public void SetZone()
    {
        for (uint i = 0; i < ZoneCount; i++)
        {
            Zones.Add(new Zone(i, this));
        }
    }

    /// <summary>
    /// Cập nhật trạng thái của bản đồ dựa trên thời gian hiện tại.
    /// </summary>
    /// <param name="time">Thời gian hiện tại (tính bằng Unix milliseconds).</param>
    public virtual void Update(long time)
    {
    }

    /// <summary>
    /// Lấy một vùng (zone) chưa đạt số lượng người chơi tối đa.
    /// </summary>
    /// <returns>Vùng thỏa mãn điều kiện hoặc null nếu không có vùng nào phù hợp.</returns>
    public Zone GetZoneNotMaxPlayer()
        => Zones.FirstOrDefault(x => x.Characters.Count < MaxPlayers);

    /// <summary>
    /// Lấy vùng (zone) chứa người chơi hiện tại.
    /// </summary>
    /// <returns>Vùng chứa người chơi hoặc null nếu không tìm thấy.</returns>
    public Zone GetZonePlayer() => Zones.FirstOrDefault(x => !x.Characters.IsEmpty);
}