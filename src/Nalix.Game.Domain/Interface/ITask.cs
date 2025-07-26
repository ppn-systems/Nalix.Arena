using Nalix.Domain.Entities;
using Nalix.Domain.Models.Tasks;
using System.Collections.Generic;

namespace Nalix.Domain.Interface;

/// <summary>
/// Giao diện định nghĩa một nhiệm vụ trong game.
/// </summary>
public interface ITask
{
    /// <summary>
    /// Mã định danh duy nhất của nhiệm vụ.
    /// </summary>
    System.UInt16 Id { get; set; }

    /// <summary>
    /// Số lượng cần hoàn thành cho nhiệm vụ.
    /// </summary>
    System.UInt16 Count { get; set; }

    /// <summary>
    /// Tên của nhiệm vụ.
    /// </summary>
    System.String Name { get; set; }

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
    List<System.String> Description { get; set; }
}