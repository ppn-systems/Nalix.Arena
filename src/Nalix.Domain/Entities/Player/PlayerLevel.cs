namespace Nalix.Domain.Entities.Player;

/// <summary>
/// Lớp đại diện cho thông tin cấp độ và kinh nghiệm của người chơi trong game.
/// </summary>
public sealed class PlayerLevel
{
    /// <summary>
    /// Cấp độ hiện tại của người chơi.
    /// </summary>
    public System.UInt32 Level { get; set; }

    /// <summary>
    /// Lượng kinh nghiệm hiện tại của người chơi.
    /// </summary>
    public System.UInt64 Experience { get; set; }
}