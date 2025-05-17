namespace Nalix.Game.Domain.Interface;

public interface IOption : System.ICloneable
{
    /// <summary>
    /// ID của Option để tra bảng dữ liệu
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Giá trị của Option, ví dụ: +10, +20%.
    /// </summary>
    public int Param { get; set; }
}