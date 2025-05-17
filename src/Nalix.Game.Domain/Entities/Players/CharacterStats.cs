namespace Nalix.Game.Domain.Entities.Players;

/// <summary>
/// Lớp chứa các chỉ số nhân vật trong trò chơi.
/// </summary>
public sealed class CharacterStats
{
    /// <summary>
    /// Sức tấn công (Damage).
    /// </summary>
    public ulong Attack { get; set; }

    /// <summary>
    /// Máu tối đa (HP).
    /// </summary>
    public ulong Health { get; set; }

    /// <summary>
    /// Năng lượng tối đa (Energy).
    /// </summary>
    public ulong Energy { get; set; }

    /// <summary>
    /// Phòng thủ.
    /// </summary>
    public ulong Defense { get; set; }

    /// <summary>
    /// Tỉ lệ chí mạng (%).
    /// </summary>
    public float CriticalRate { get; set; }
}