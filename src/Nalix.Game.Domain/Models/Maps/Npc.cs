using Nalix.Game.Domain.Common;
using Nalix.Game.Domain.Common.Enums;
using Nalix.Game.Domain.Entities;

namespace Nalix.Game.Domain.Models.Maps;

/// <summary>
/// Đại diện cho một NPC trong trò chơi.
/// </summary>
public class Npc : NamedEntity<ushort>
{
    /// <summary>
    /// ID của avatar hoặc sprite của NPC.
    /// </summary>
    public ushort AvatarId { get; set; }

    /// <summary>
    /// Loại của NPC (Thương nhân, Người giao nhiệm vụ, v.v.).
    /// </summary>
    public NpcType Type { get; set; }

    /// <summary>
    /// Trạng thái hiện tại của NPC (Đứng yên, v.v.).
    /// </summary>
    public NpcState State { get; set; }

    /// <summary>
    /// Vị trí của NPC trên bản đồ.
    /// </summary>
    public Position Position { get; set; }

    /// <summary>
    /// Xác định liệu người chơi có thể tương tác với NPC hay không.
    /// </summary>
    public bool IsInteractive { get; set; }

    /// <summary>
    /// Các đoạn hội thoại mà NPC có thể nói.
    /// </summary>
    public string[] Dialogues { get; set; } = [];
}