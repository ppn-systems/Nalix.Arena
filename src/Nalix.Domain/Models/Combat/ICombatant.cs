using Nalix.Domain.Entities.Player;

namespace Nalix.Domain.Models.Combat;

/// <summary>
/// Định nghĩa giao diện cho các thực thể tham gia chiến đấu.
/// </summary>
public interface ICombatant
{
    /// <summary>
    ///
    /// </summary>
    CharacterStats CharacterStats { get; set; }

    /// <summary>
    /// Xác định xem chiến binh còn sống hay không.
    /// </summary>
    System.Boolean IsAlive => CharacterStats.Health > 0;

    /// <summary>
    /// Gây sát thương lên chiến binh.
    /// </summary>
    /// <param name="amount">Lượng sát thương cần áp dụng.</param>
    void TakeDamage(System.Int64 amount);

    /// <summary>
    /// Tính toán lượng sát thương có thể gây lên mục tiêu.
    /// </summary>
    /// <param name="target">Mục tiêu bị tấn công.</param>
    /// <returns>Lượng sát thương có thể gây lên mục tiêu.</returns>
    System.Int64 CalculateDamage(ICombatant target);
}