namespace Nalix.Game.Domain.Interface;

public interface IOption : System.ICloneable
{
    /// <summary>
    /// ID của Option để tra bảng dữ liệu
    /// </summary>
    System.Int32 Id { get; set; }

    /// <summary>
    /// Giá trị của Option, ví dụ: +10, +20%.
    /// </summary>
    System.Int32 Param { get; set; }
}