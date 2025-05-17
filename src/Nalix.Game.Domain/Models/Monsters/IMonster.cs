using Nalix.Game.Domain.Entities;

namespace Nalix.Game.Domain.Models.Monsters;

/// <summary>
/// Giao diện định nghĩa các thuộc tính và hành vi cơ bản của một quái vật trong game.
/// </summary>
public interface IMonster
{
    /// <summary>
    /// Mã định danh duy nhất của quái vật.
    /// </summary>
    uint Id { get; set; }

    /// <summary>
    /// Tên của quái vật.
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// Kiểm tra xem quái vật còn sống hay không (dựa trên chỉ số máu).
    /// </summary>
    bool IsAlive { get; }

    /// <summary>
    /// Thời điểm tấn công tiếp theo của quái vật (dựa trên Unix milliseconds).
    /// </summary>
    long TimeAttack { get; set; }

    /// <summary>
    /// Vị trí hiện tại của quái vật trên bản đồ.
    /// </summary>
    Position Position { get; set; }

    /// <summary>
    /// Các chỉ số của quái vật (máu, sát thương, phòng thủ...).
    /// </summary>
    MonsterStats Stats { get; set; }

    /// <summary>
    /// Bảng vật phẩm rơi ra khi quái vật bị tiêu diệt.
    /// </summary>
    LootTable Loot { get; set; }

    /// <summary>
    /// Thông tin về thời gian và cách làm mới quái vật trên bản đồ.
    /// </summary>
    RefreshInfo Refresh { get; set; }

    /// <summary>
    /// Loại quái vật (ví dụ: cận chiến, tầm xa...).
    /// </summary>
    MonsterType Type { get; set; }
}