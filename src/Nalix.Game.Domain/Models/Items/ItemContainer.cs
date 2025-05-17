using Nalix.Logging;
using System.Collections.Generic;
using System.Linq;

namespace Nalix.Game.Domain.Models.Items;

public sealed class ItemContainer
{
    private readonly List<Item> _items = [];

    // Maximum number of items allowed in the chest.
    public int MaxCapacity { get; set; } = 100;

    /// <summary>
    /// Lấy danh sách tất cả các vật phẩm trong rương đồ.
    /// </summary>
    public IReadOnlyList<Item> Items => _items.AsReadOnly();

    /// <summary>
    /// Thêm một vật phẩm vào rương đồ.
    /// Nếu vật phẩm đã tồn tại, sẽ tăng số lượng.
    /// </summary>
    /// <param name="item">Vật phẩm cần thêm vào.</param>
    /// <returns>True nếu thêm thành công, False nếu vượt quá giới hạn.</returns>
    public bool AddItem(Item item)
    {
        // Kiểm tra nếu thêm vật phẩm sẽ vượt quá dung lượng tối đa của rương đồ.
        if (_items.Count >= MaxCapacity)
        {
            NLogix.Host.Instance.Debug("[ItemContainer.AddItem] The inventory is full.");
            return false;  // Không thể thêm nếu rương đồ đã đầy.
        }

        // Thêm vật phẩm vào rương đồ hoặc tăng số lượng nếu đã tồn tại.
        var existingItem = _items.FirstOrDefault(i => i.Id == item.Id);
        if (existingItem != null)
        {
            existingItem.Quantity += item.Quantity;  // Tăng số lượng vật phẩm.
        }
        else
        {
            _items.Add(item);  // Thêm vật phẩm mới vào rương.
        }

        return true;  // Thêm vật phẩm thành công.
    }

    /// <summary>
    /// Loại bỏ một số lượng vật phẩm khỏi rương đồ.
    /// </summary>
    /// <param name="itemId">Mã vật phẩm cần loại bỏ.</param>
    /// <param name="quantity">Số lượng cần loại bỏ.</param>
    /// <returns>True nếu loại bỏ thành công, False nếu không thể loại bỏ (ví dụ: số lượng không đủ).</returns>
    public bool RemoveItem(short itemId, int quantity)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item == null || item.Quantity < quantity)
        {
            return false;  // Không tìm thấy vật phẩm hoặc số lượng không đủ.
        }

        item.Quantity -= quantity;  // Giảm số lượng vật phẩm.
        if (item.Quantity == 0)
        {
            _items.Remove(item);  // Xóa vật phẩm nếu số lượng là 0.
        }

        return true;  // Loại bỏ thành công.
    }

    /// <summary>
    /// Tìm kiếm vật phẩm theo mã định danh.
    /// </summary>
    /// <param name="itemId">Mã định danh của vật phẩm.</param>
    /// <returns>Vật phẩm nếu tìm thấy, null nếu không tìm thấy.</returns>
    public Item GetItem(short itemId) => _items.FirstOrDefault(i => i.Id == itemId);

    /// <summary>
    /// Kiểm tra xem rương đồ có chứa vật phẩm với mã định danh cụ thể hay không.
    /// </summary>
    /// <param name="itemId">Mã định danh của vật phẩm cần kiểm tra.</param>
    /// <returns>True nếu vật phẩm có trong rương đồ, False nếu không có.</returns>
    public bool ContainsItem(short itemId) => _items.Any(i => i.Id == itemId);
}