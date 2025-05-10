using System.Collections.Generic;

namespace Nalix.Game.Domain.Models.Tasks;

public interface ITask
{
    short Id { get; set; }
    short Count { get; set; }
    string Name { get; set; }                       // Tên nhiệm vụ
    Reward Reward { get; set; }                     // Phần thưởng nhiệm vụ
    TaskType Type { get; set; }                     // Loại nhiệm vụ
    List<string> Description { get; set; }          // Mô tả nhiệm vụ
}