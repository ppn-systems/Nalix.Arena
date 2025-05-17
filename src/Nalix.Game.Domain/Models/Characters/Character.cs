using Nalix.Game.Domain.Entities;
using Nalix.Game.Domain.Models.Items;

namespace Nalix.Game.Domain.Models.Characters;

/// <summary>
/// Đại diện cho một nhân vật trong trò chơi.
/// </summary>
public sealed class Character
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
}