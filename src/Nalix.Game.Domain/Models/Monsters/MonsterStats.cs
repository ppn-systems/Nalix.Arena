namespace Nalix.Game.Domain.Models.Monsters;

/// <summary>
/// Chỉ số phi chiến đấu (non-combat) của quái vật.
/// </summary>
public class MonsterStats
{
    /// <summary>
    /// Cấp độ của quái vật.
    /// </summary>
    public System.UInt32 Level { get; set; }

    /// <summary>
    /// Sức khỏe tối đa của quái vật.
    /// </summary>
    public System.UInt32 MaxHealth { get; set; }

    /// <summary>
    /// Lượng kinh nghiệm rơi ra khi bị tiêu diệt.
    /// </summary>
    public System.UInt32 Experience { get; set; }
}