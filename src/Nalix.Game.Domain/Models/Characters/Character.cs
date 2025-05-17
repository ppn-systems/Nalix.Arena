using Nalix.Game.Domain.Entities;
using Nalix.Game.Domain.Models.Combat;
using Nalix.Game.Domain.Models.Items;

namespace Nalix.Game.Domain.Models.Characters;

/// <summary>
/// Đại diện cho một nhân vật trong trò chơi.
/// </summary>
public sealed class Character : ICombatant
{
    /// <summary>
    /// ID của bản đồ mà nhân vật đang ở.
    /// </summary>
    public int MapId { get; set; }

    /// <summary>
    /// Vị trí hiện tại của nhân vật trên bản đồ.
    /// </summary>
    public Position Position { get; set; }

    /// <summary>
    /// Các chỉ số của nhân vật, bao gồm sức mạnh, phòng thủ, tốc độ, v.v.
    /// </summary>
    public CharacterStats CharacterStats { get; set; }

    // Inventory and Chest for the player

    /// <summary>
    /// Tiền tệ của người chơi.
    /// </summary>
    public Currency Currency { get; set; }

    /// <summary>
    /// Rương chứa vật phẩm của nhân vật.
    /// </summary>
    public ItemContainer Chest { get; set; }

    /// <summary>
    /// Túi đồ (kho vật phẩm) của nhân vật.
    /// </summary>
    public ItemContainer Inventory { get; set; }

    /// <summary>
    /// Tính sát thương gây ra cho mục tiêu dựa vào chỉ số tấn công và giáp của mục tiêu.
    /// </summary>
    public long CalculateDamage(ICombatant target)
    {
        long targetArmor = 0;

        if (target is ICombatant combatant)
        {
            targetArmor = combatant.CharacterStats.Defense;
        }

        long damage = CharacterStats.Attack - targetArmor;

        // Đảm bảo sát thương tối thiểu là 1
        return damage < 1 ? 1 : damage;
    }

    /// <summary>
    /// Nhận sát thương từ kẻ thù và giảm máu tương ứng.
    /// </summary>
    public void TakeDamage(long amount)
    {
        CharacterStats.Health -= amount;

        if (CharacterStats.Health < 0)
        {
            CharacterStats.Health = 0; // Không cho xuống âm
        }
    }
}