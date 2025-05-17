using Nalix.Game.Domain.Entities;
using System.Collections.Generic;

namespace Nalix.Game.Domain.Models.Tasks;

/// <summary>
/// Giao diện định nghĩa một nhiệm vụ trong game.
/// </summary>
public interface ITask
{
    /// <summary>
    /// Mã định danh duy nhất của nhiệm vụ.
    /// </summary>
    ushort Id { get; set; }

    /// <summary>
    /// Số lượng cần hoàn thành cho nhiệm vụ.
    /// </summary>
    ushort Count { get; set; }

    /// <summary>
    /// Tên của nhiệm vụ.
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// Phần thưởng nhận được khi hoàn thành nhiệm vụ.
    /// </summary>
    Reward Reward { get; set; }

    /// <summary>
    /// Loại nhiệm vụ (ví dụ: nhiệm vụ chính, phụ, hàng ngày...).
    /// </summary>
    TaskType Type { get; set; }

    /// <summary>
    /// Danh sách mô tả chi tiết về nhiệm vụ.
    /// </summary>
    List<string> Description { get; set; }
}