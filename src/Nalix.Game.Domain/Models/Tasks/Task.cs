using Nalix.Game.Domain.Entities;
using System.Collections.Generic;

namespace Nalix.Game.Domain.Models.Tasks;

public class Task : ITask
{
    public short Id { get; set; }
    public short Count { get; set; }
    public string Name { get; set; }                       // Tên nhiệm vụ
    public Reward Reward { get; set; }                     // Phần thưởng nhiệm vụ
    public TaskType Type { get; set; }                     // Loại nhiệm vụ
    public List<string> Description { get; set; }          // Mô tả nhiệm vụ
}