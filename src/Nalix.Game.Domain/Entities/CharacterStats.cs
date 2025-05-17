namespace Nalix.Game.Domain.Entities;

/// <summary>
/// Lớp chứa các chỉ số nhân vật trong trò chơi.
/// </summary>
public sealed class CharacterStats
{
    /// <summary>
    /// Sức tấn công (Damage).
    /// </summary>
    public long Attack { get; set; }

    /// <summary>
    /// Máu tối đa (HP).
    /// </summary>
    public long Health { get; set; }

    /// <summary>
    /// Năng lượng tối đa (Energy).
    /// </summary>
    public long Energy { get; set; }

    /// <summary>
    /// Phòng thủ.
    /// </summary>
    public long Defense { get; set; }

    /// <summary>
    /// Tỉ lệ chí mạng (%).
    /// </summary>
    public float CriticalRate { get; set; }
}