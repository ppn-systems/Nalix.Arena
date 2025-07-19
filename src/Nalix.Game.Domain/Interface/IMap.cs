using Nalix.Game.Domain.Models.Maps;
using Nalix.Game.Domain.Models.Maps.Items;
using Nalix.Game.Domain.Models.Monsters;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nalix.Game.Domain.Interface;

/// <summary>
/// Giao diện định nghĩa các thuộc tính và hành vi của một bản đồ trong game.
/// </summary>
public interface IMap
{
    /// <summary>
    /// Mã định danh duy nhất của bản đồ.
    /// </summary>
    System.UInt32 Id { get; }

    /// <summary>
    /// Tên của bản đồ.
    /// </summary>
    System.String Name { get; set; }

    /// <summary>
    /// Thời gian liên quan đến bản đồ (có thể là thời gian tạo hoặc cập nhật, tính bằng Unix milliseconds).
    /// </summary>
    System.Int64 TimeMap { get; set; }

    /// <summary>
    /// Số lượng vùng (zone) trong bản đồ.
    /// </summary>
    System.Byte ZoneCount { get; set; }

    /// <summary>
    /// Trạng thái hoạt động của bản đồ (đang chạy hay không).
    /// </summary>
    System.Boolean IsRunning { get; set; }

    /// <summary>
    /// Số lượng người chơi tối đa được phép trên bản đồ.
    /// </summary>
    System.Byte MaxPlayers { get; set; }

    /// <summary>
    /// Bản đồ ô (tile map) chứa thông tin về cấu trúc ô của bản đồ.
    /// </summary>
    TileMap TileMap { get; }

    /// <summary>
    /// Danh sách các vùng (zone) trong bản đồ.
    /// </summary>
    List<Zone> Zones { get; }

    /// <summary>
    /// Tác vụ xử lý các vùng trong bản đồ.
    /// </summary>
    Task HandleZone { get; set; }

    /// <summary>
    /// Danh sách các NPC (nhân vật không chơi) trên bản đồ.
    /// </summary>
    List<Npc> Npcs { get; set; }

    /// <summary>
    /// Danh sách các quái vật trên bản đồ.
    /// </summary>
    List<Monster> Monsters { get; set; }

    /// <summary>
    /// Danh sách các điểm cách (waypoint) trên bản đồ.
    /// </summary>
    List<WayPoint> WayPoints { get; set; }

    /// <summary>
    /// Danh sách các vật phẩm hành động (action items) trên bản đồ.
    /// </summary>
    List<ActionItem> ActionItems { get; set; }

    /// <summary>
    /// Danh sách các vật phẩm nền (background items) trên bản đồ.
    /// </summary>
    List<BackgroundItem> BackgroundItems { get; set; }

    /// <summary>
    /// Thiết lập các vùng (zone) cho bản đồ.
    /// </summary>
    void SetZone();

    /// <summary>
    /// Lấy một vùng (zone) chưa đạt số lượng người chơi tối đa.
    /// </summary>
    /// <returns>Vùng thỏa mãn điều kiện hoặc null nếu không có.</returns>
    Zone GetZoneNotMaxPlayer();

    /// <summary>
    /// Lấy vùng (zone) chứa người chơi hiện tại.
    /// </summary>
    /// <returns>Vùng chứa người chơi hoặc null nếu không tìm thấy.</returns>
    Zone GetZonePlayer();
}