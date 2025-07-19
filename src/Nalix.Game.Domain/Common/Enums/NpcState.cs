namespace Nalix.Game.Domain.Common.Enums;

/// <summary>
/// Xác định trạng thái của NPC trong trò chơi.
/// </summary>
public enum NpcState : System.Byte
{
    /// <summary>
    /// NPC đang đứng yên, không thực hiện hành động nào.
    /// </summary>
    Idle = 0,

    /// <summary>
    /// NPC bị ẩn, không hiển thị trên bản đồ.
    /// </summary>
    Hidden = 1,

    /// <summary>
    /// NPC đang ngủ hoặc không hoạt động.
    /// </summary>
    Sleeping = 2
}