using Nalix.Domain.Models.Items;
using System.Collections.Generic;

namespace Nalix.Domain.Entities;

/// <summary>
/// Lớp đại diện cho phần thưởng nhận được trong game.
/// </summary>
public class Reward
{
    /// <summary>
    /// Lượng kinh nghiệm (EXP) nhận được từ phần thưởng.
    /// </summary>
    public System.UInt32 Experience { get; set; }

    /// <summary>
    /// Lượng vàng nhận được từ phần thưởng.
    /// </summary>
    public System.UInt32 Gold { get; set; }

    /// <summary>
    /// Lượng kim cương nhận được từ phần thưởng.
    /// </summary>
    public System.UInt32 Gems { get; set; }

    /// <summary>
    /// Danh sách các vật phẩm nhận được từ phần thưởng.
    /// </summary>
    public List<Item> Items { get; set; } = [];
}