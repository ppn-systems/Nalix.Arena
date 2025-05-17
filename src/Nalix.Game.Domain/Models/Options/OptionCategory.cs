namespace Nalix.Game.Domain.Models.Options;

/// <summary>
/// Danh mục các loại tùy chọn trong game.
/// </summary>
public enum OptionCategory : byte
{
    /// <summary>
    /// Không xác định.
    /// </summary>
    Unknown,

    /// <summary>
    /// Thuộc tính cơ bản (máu, sát thương, tốc độ...).
    /// </summary>
    Stat,

    /// <summary>
    /// Hiệu ứng có lợi (buff).
    /// </summary>
    Buff,

    /// <summary>
    /// Hiệu ứng bất lợi (debuff).
    /// </summary>
    Debuff,

    /// <summary>
    /// Tăng cường hiệu ứng kỹ năng.
    /// </summary>
    SkillModifier,

    /// <summary>
    /// Nguyên tố (lửa, băng, độc...).
    /// </summary>
    Element,

    /// <summary>
    /// Tùy chọn đặc biệt (không cộng dồn, sử dụng một lần...).
    /// </summary>
    Special
}