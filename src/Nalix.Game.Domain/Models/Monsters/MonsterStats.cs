namespace Nalix.Game.Domain.Models.Monsters;

/// <summary>
/// Lớp đại diện cho các chỉ số cơ bản của một quái vật trong game.
/// </summary>
public class MonsterStats
{
    /// <summary>
    /// Cấp độ của quái vật.
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    /// Chỉ số phòng thủ của quái vật.
    /// </summary>
    public int Armor { get; set; }

    /// <summary>
    /// Sát thương mà quái vật gây ra.
    /// </summary>
    public int Damage { get; set; }

    /// <summary>
    /// Sức khỏe hiện tại của quái vật.
    /// </summary>
    public int Health { get; set; }

    /// <summary>
    /// Sức khỏe tối đa của quái vật.
    /// </summary>
    public int MaxHealth { get; set; }

    /// <summary>
    /// Lượng kinh nghiệm mà quái vật cung cấp khi bị tiêu diệt.
    /// </summary>
    public int Experience { get; set; }
}