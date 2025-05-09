using Nalix.Game.Domain.Models.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nalix.Game.Domain.Models.Items;

/// <summary>
/// Đại diện cho một vật phẩm trong trò chơi, bao gồm thông tin giá cả, số lượng và các tùy chọn bổ sung.
/// </summary>
public class Item : IDisposable
{
    /// <summary>
    /// Lấy hoặc đặt mã định danh duy nhất của vật phẩm.
    /// </summary>
    /// <example>Ví dụ: 101 cho một thanh kiếm cụ thể.</example>
    public short Id { get; set; }

    /// <summary>
    /// Lấy hoặc đặt chỉ số giao diện người dùng (UI) của vật phẩm.
    /// </summary>
    /// <example>Ví dụ: 1 cho vị trí đầu tiên trong danh sách UI.</example>
    public int IndexUI { get; set; }

    /// <summary>
    /// Lấy hoặc đặt giá mua vật phẩm bằng vàng.
    /// </summary>
    /// <example>Ví dụ: 200 vàng để mua vật phẩm.</example>
    public int BuyGold { get; set; } = 0;

    /// <summary>
    /// Lấy hoặc đặt giá mua vật phẩm bằng xu.
    /// </summary>
    /// <example>Ví dụ: 50 xu để mua vật phẩm.</example>
    public int BuyGem { get; set; } = 0;

    /// <summary>
    /// Lấy hoặc đặt giá bán vật phẩm bằng vàng.
    /// </summary>
    /// <example>Ví dụ: 100 vàng để bán vật phẩm.</example>
    public int SaleGold { get; set; } = 1;

    /// <summary>
    /// Lấy hoặc đặt số lượng vật phẩm.
    /// </summary>
    /// <example>Ví dụ: 5 cho 5 đơn vị vật phẩm.</example>
    public int Quantity { get; set; } = 1;

    /// <summary>
    /// Lấy hoặc đặt lý do hoặc mô tả liên quan đến vật phẩm.
    /// </summary>
    /// <example>Ví dụ: "Nhận từ nhiệm vụ chính."</example>
    public string Reason { get; set; }

    /// <summary>
    /// Lấy hoặc đặt danh sách các tùy chọn bổ sung của vật phẩm.
    /// </summary>
    /// <example>Ví dụ: Danh sách chứa các <see cref="OptionItem"/> như tăng sức mạnh hoặc tốc độ.</example>
    public List<OptionItem> Options { get; set; }

    /// <summary>
    /// Khởi tạo một vật phẩm mới với các giá trị mặc định.
    /// </summary>
    /// <remarks>
    /// Thuộc tính <see cref="Reason"/> được đặt thành chuỗi rỗng và <see cref="Options"/> được khởi tạo thành danh sách rỗng.
    /// </remarks>
    public Item()
    {
        Reason = "";
        Options = [];
    }

    /// <summary>
    /// Lấy giá trị tham số của một tùy chọn dựa trên mã định danh.
    /// </summary>
    /// <param name="id">Mã định danh của tùy chọn cần tìm.</param>
    /// <returns>Giá trị tham số của tùy chọn nếu tìm thấy; nếu không, trả về 0.</returns>
    /// <example>
    /// Ví dụ: Nếu <paramref name="id"/> là 1 và tùy chọn có <see cref="OptionItem.Param"/> là 10, hàm trả về 10.
    /// </example>
    public int GetParamOption(int id)
    {
        OptionItem option = Options.FirstOrDefault(op => op.Id == id);
        return option != null ? option.Param : 0;
    }

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

    public void Dispose() => GC.SuppressFinalize(this);
}