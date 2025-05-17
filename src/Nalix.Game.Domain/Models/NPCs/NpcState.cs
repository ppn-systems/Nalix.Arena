namespace Nalix.Game.Domain.Models.NPCs;

/// <summary>
/// Xác định trạng thái của NPC trong trò chơi.
/// </summary>
public enum NpcState : byte
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