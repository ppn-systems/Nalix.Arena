namespace Nalix.Game.Presentation.Enums;

/// <summary>
/// Định nghĩa các mức độ ưu tiên vẽ (Z-Index) cho các đối tượng trong trò chơi.
/// Giá trị cao hơn sẽ được vẽ lên trên các đối tượng có giá trị thấp hơn.
/// </summary>
public enum ZIndex : System.Int32
{
    /// <summary>
    /// Mức độ ưu tiên thấp nhất, dùng cho nền (background).
    /// </summary>
    Background = 0,

    /// <summary>
    /// Mức độ ưu tiên cho banner cuộn.
    /// </summary>
    Banner = 9,

    /// <summary>
    /// Mức độ ưu tiên cho giao diện cài đặt.
    /// </summary>
    Settings = 10,

    /// <summary>
    /// Mức độ ưu tiên cho hộp thoại.
    /// </summary>
    Dialog = 11,

    /// <summary>
    /// Mức độ ưu tiên cho tooltip (gợi ý).
    /// </summary>
    Tooltip = 13,

    /// <summary>
    /// Mức độ ưu tiên cho lớp phủ (overlay), cao hơn hầu hết các đối tượng.
    /// </summary>
    Overlay = System.Int32.MaxValue - 2,

    /// <summary>
    /// Mức độ ưu tiên cho thông báo, cao hơn lớp phủ.
    /// </summary>
    Notification = System.Int32.MaxValue - 1,

    /// <summary>
    /// Mức độ ưu tiên cao nhất, dùng cho đối tượng hiển thị trên cùng.
    /// </summary>
    Topmost = System.Int32.MaxValue
}

/// <summary>
/// Cung cấp các phương thức mở rộng (extension methods) cho enum <see cref="ZIndex"/>.
/// </summary>
public static class ZIndexExtensions
{
    /// <summary>
    /// Chuyển đổi giá trị của <see cref="ZIndex"/> thành kiểu <see cref="System.Int32"/>.
    /// </summary>
    /// <param name="zIndex">Giá trị <see cref="ZIndex"/> cần chuyển đổi.</param>
    /// <returns>Giá trị số nguyên tương ứng với <see cref="ZIndex"/>.</returns>
    public static System.Int32 ToInt(this ZIndex zIndex) => (System.Int32)zIndex;
}