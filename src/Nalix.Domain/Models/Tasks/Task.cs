using Nalix.Domain.Common;
using Nalix.Domain.Entities;
using Nalix.Domain.Interface;
using System.Collections.Generic;

namespace Nalix.Domain.Models.Tasks;

/// <summary>
/// Lớp đại diện cho một nhiệm vụ trong game, triển khai giao diện <see cref="ITask"/>.
/// </summary>
public class Task : NamedEntity<System.UInt16>, ITask
{
    /// <summary>
    /// Số lượng cần hoàn thành cho nhiệm vụ.
    /// </summary>
    public System.UInt16 Count { get; set; }

    /// <summary>
    /// Phần thưởng nhận được khi hoàn thành nhiệm vụ.
    /// </summary>
    public Reward Reward { get; set; }

    /// <summary>
    /// Loại nhiệm vụ (ví dụ: nhiệm vụ chính, phụ, hàng ngày...).
    /// </summary>
    public TaskType Type { get; set; }

    /// <summary>
    /// Danh sách mô tả chi tiết về nhiệm vụ.
    /// </summary>
    public List<System.String> Description { get; set; }
}