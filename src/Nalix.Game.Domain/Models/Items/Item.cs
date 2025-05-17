using Nalix.Game.Domain.Models.Options;
using System.Collections.Generic;
using System.Linq;

namespace Nalix.Game.Domain.Models.Items;

/// <summary>
/// Đại diện cho một vật phẩm trong trò chơi, bao gồm thông tin giá cả, số lượng và các tùy chọn bổ sung.
/// </summary>
public sealed class Item
{
    /// <summary>
    /// Lấy hoặc đặt mã định danh duy nhất của vật phẩm.
    /// </summary>
    public short Id { get; set; }

    /// <summary>
    /// Lấy hoặc đặt chỉ số giao diện người dùng (UI) của vật phẩm.
    /// </summary>
    public int IndexUI { get; set; }

    /// <summary>
    /// Lấy hoặc đặt tên của vật phẩm.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Lấy hoặc đặt giá mua vật phẩm bằng vàng.
    /// </summary>
    public int Buy { get; set; } = 0;

    /// <summary>
    /// Lấy hoặc đặt giá bán vật phẩm bằng vàng.
    /// </summary>
    public int Sale { get; set; } = 1;

    /// <summary>
    /// Lấy hoặc đặt số lượng vật phẩm.
    /// </summary>
    public int Quantity { get; set; } = 1;

    /// <summary>
    /// Lấy hoặc đặt danh sách các tùy chọn bổ sung của vật phẩm.
    /// </summary>
    /// <example>Ví dụ: Danh sách chứa các <see cref="OptionItem"/> như tăng sức mạnh hoặc tốc độ.</example>
    public List<OptionItem> Options { get; set; } = [];

    /// <summary>
    /// Lấy giá trị tham số của một tùy chọn dựa trên mã định danh.
    /// </summary>
    /// <param name="id">Mã định danh của tùy chọn cần tìm.</param>
    /// <returns>Giá trị tham số của tùy chọn nếu tìm thấy; nếu không, trả về 0.</returns>
    /// <example>
    /// Ví dụ: Nếu <paramref name="id"/> là 1 và tùy chọn có <see cref="OptionItem.Param"/> là 10, hàm trả về 10.
    /// </example>
    public int GetParamOption(int id) =>
        Options.FirstOrDefault(op => op.Id == id)?.Param ?? 0;

    /// <summary>
    /// Kiểm tra xem vật phẩm có chứa một tùy chọn cụ thể hay không.
    /// </summary>
    /// <param name="id">Mã định danh của tùy chọn cần kiểm tra.</param>
    /// <returns>True nếu vật phẩm có tùy chọn với <paramref name="id"/>; nếu không, trả về false.</returns>
    /// <example>
    /// Ví dụ: Trả về true nếu vật phẩm có tùy chọn với <paramref name="id"/> là 1.
    /// </example>
    public bool IsHaveOption(int id)
        => Options.FirstOrDefault(op => op.Id == id) != null;
}