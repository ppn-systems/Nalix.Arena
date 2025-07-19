namespace Nalix.Game.Domain.Entities;

/// <summary>
/// Lớp chứa các chỉ số nhân vật trong trò chơi.
/// </summary>
public sealed class CharacterStats
{
    /// <summary>
    /// Sức tấn công (Damage).
    /// </summary>
    public System.Int64 Attack { get; set; }

    /// <summary>
    /// Máu tối đa (HP).
    /// </summary>
    public System.Int64 Health { get; set; }

    /// <summary>
    /// Năng lượng tối đa (Energy).
    /// </summary>
    public System.Int64 Energy { get; set; }

    /// <summary>
    /// Phòng thủ.
    /// </summary>
    public System.Int64 Defense { get; set; }

    /// <summary>
    /// Tỉ lệ chí mạng (%).
    /// </summary>
    public System.Single CriticalRate { get; set; }
}