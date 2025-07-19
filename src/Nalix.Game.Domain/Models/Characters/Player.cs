using Nalix.Common.Connection;

namespace Nalix.Game.Domain.Models.Characters;

/// <summary>
/// Đại diện cho một người chơi trong trò chơi.
/// </summary>
public sealed class Player
{
    /// <summary>
    /// ID duy nhất của người chơi.
    /// </summary>
    public System.Int32 Id { get; set; }

    /// <summary>
    /// Tên của người chơi.
    /// </summary>
    public System.String Name { get; set; }

    /// <summary>
    /// Nhân vật mà người chơi điều khiển.
    /// </summary>
    public Character Character { get; set; }

    /// <summary>
    /// Kết nối của người chơi với hệ thống.
    /// </summary>
    public IConnection Connection { get; set; }
}