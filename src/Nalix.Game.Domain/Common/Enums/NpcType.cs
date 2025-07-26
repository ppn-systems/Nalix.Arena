namespace Nalix.Domain.Common.Enums;

/// <summary>
/// Xác định loại của NPC trong trò chơi.
/// </summary>
public enum NpcType : System.Byte
{
    /// <summary>
    /// NPC thông thường, không có chức năng đặc biệt.
    /// </summary>
    Normal = 0,

    /// <summary>
    /// NPC giao nhiệm vụ cho người chơi.
    /// </summary>
    QuestGiver = 1,

    /// <summary>
    /// NPC thương nhân, có thể mua bán vật phẩm.
    /// </summary>
    Merchant = 2,

    /// <summary>
    /// NPC huấn luyện viên, có thể giúp nâng cấp kỹ năng hoặc chỉ số.
    /// </summary>
    Trainer = 3
}