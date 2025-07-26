using Nalix.Domain.Interface;

namespace Nalix.Domain.Models.Options;

public sealed class OptionItem : IOption
{
    /// <inheritdoc />
    public System.Int32 Id { get; set; }

    /// <inheritdoc />
    public System.Int32 Param { get; set; }

    /// <summary>
    /// Thời gian tồn tại của Option nếu là Buff/Debuff (ms)
    /// </summary>
    public System.Int64 Duration { get; set; } = -1; // -1 = Vĩnh viễn

    /// <summary>
    /// Phân loại Option, hỗ trợ hệ thống xử lý linh hoạt.
    /// </summary>
    public OptionCategory Category { get; set; }

    /// <summary>
    /// Cho biết Option có thể stack hay không
    /// </summary>
    public System.Boolean IsStackable { get; set; } = true;

    public System.Object Clone()
        => new OptionItem()
        {
            Id = Id,
            Param = Param,
            Duration = Duration,
            Category = Category,
            IsStackable = IsStackable
        };
}