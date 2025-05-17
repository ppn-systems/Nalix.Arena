namespace Nalix.Game.Domain.Models.Monsters;

/// <summary>
/// Danh mục các loại quái vật trong game.
/// </summary>
public enum MonsterType : byte
{
    /// <summary>
    /// Quái vật cận chiến, tấn công ở cự ly gần.
    /// </summary>
    Melee,

    /// <summary>
    /// Quái vật tầm xa, tấn công từ khoảng cách.
    /// </summary>
    Ranged,

    /// <summary>
    /// Quái vật sử dụng phép thuật để tấn công.
    /// </summary>
    Magic,

    /// <summary>
    /// Quái vật cấp trùm, có sức mạnh vượt trội.
    /// </summary>
    Boss,

    /// <summary>
    /// Quái vật thuộc nguyên tố lửa, có khả năng gây sát thương lửa.
    /// </summary>
    Fire,

    /// <summary>
    /// Quái vật thuộc nguyên tố băng, có khả năng gây sát thương băng.
    /// </summary>
    Ice,

    /// <summary>
    /// Quái vật thuộc nguyên tố độc, có khả năng gây sát thương độc.
    /// </summary>
    Poison
}