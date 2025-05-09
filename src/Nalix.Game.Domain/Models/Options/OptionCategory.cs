namespace Nalix.Game.Domain.Models.Options;

public enum OptionCategory : byte
{
    Unknown,
    Stat,         // Thuộc tính (máu, damage, speed...)
    Buff,         // Buff hiệu ứng
    Debuff,       // Gây bất lợi
    SkillModifier,// Tăng hiệu ứng kỹ năng
    Element,      // Nguyên tố (lửa, băng, độc...)
    Special       // Đặc biệt (không stack, dùng 1 lần...)
}