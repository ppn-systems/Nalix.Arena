using Nalix.Game.Domain.Interface;

namespace Nalix.Game.Domain.Models.Options;

public sealed class OptionItem : IOption
{
    /// <inheritdoc />
    public int Id { get; set; }

    /// <inheritdoc />
    public int Param { get; set; }

    /// <summary>
    /// Thời gian tồn tại của Option nếu là Buff/Debuff (ms)
    /// </summary>
    public long Duration { get; set; } = -1; // -1 = Vĩnh viễn

    /// <summary>
    /// Phân loại Option, hỗ trợ hệ thống xử lý linh hoạt.
    /// </summary>
    public OptionCategory Category { get; set; }

    /// <summary>
    /// Cho biết Option có thể stack hay không
    /// </summary>
    public bool IsStackable { get; set; } = true;

    public object Clone()
        => new OptionItem()
        {
            Id = Id,
            Param = Param,
            Duration = Duration,
            Category = Category,
            IsStackable = IsStackable
        };
}