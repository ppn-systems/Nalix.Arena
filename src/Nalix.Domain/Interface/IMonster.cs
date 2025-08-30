using Nalix.Domain.Entities.Economy;
using Nalix.Domain.Entities.Player;
using Nalix.Domain.Models.Attacks;
using Nalix.Domain.Models.Combat;
using Nalix.Domain.Models.Monsters;

namespace Nalix.Domain.Interface;

/// <summary>
/// Giao diện định nghĩa các thuộc tính và hành vi cơ bản của một quái vật trong game.
/// </summary>
public interface IMonster : ICombatant
{
    /// <summary>
    /// Mã định danh duy nhất của quái vật.
    /// </summary>
    System.UInt32 Id { get; set; }

    /// <summary>
    /// Tên của quái vật.
    /// </summary>
    System.String Name { get; set; }

    /// <summary>
    /// Thời điểm tấn công tiếp theo của quái vật (dựa trên Unix milliseconds).
    /// </summary>
    System.Int64 TimeAttack { get; set; }

    /// <summary>
    /// Vị trí hiện tại của quái vật trên bản đồ.
    /// </summary>
    Position Position { get; set; }

    /// <summary>
    /// Các chỉ số của quái vật (máu, sát thương, phòng thủ...).
    /// </summary>
    MonsterStats MonsterStats { get; set; }

    /// <summary>
    /// Bảng vật phẩm rơi ra khi quái vật bị tiêu diệt.
    /// </summary>
    LootTable Loot { get; set; }

    /// <summary>
    /// Thông tin về thời gian và cách làm mới quái vật trên bản đồ.
    /// </summary>
    RefreshInfo Refresh { get; set; }

    /// <summary>
    /// Hành vi tấn công cận chiến của quái vật.
    /// </summary>
    IAttackBehavior MeleeAttack { get; set; }

    /// <summary>
    /// Hành vi tấn công tầm xa của quái vật.
    /// </summary>
    IAttackBehavior RangedAttack { get; set; }
}