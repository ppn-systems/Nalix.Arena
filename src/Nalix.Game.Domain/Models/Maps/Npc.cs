using Nalix.Game.Domain.Shared;

namespace Nalix.Game.Domain.Models.Maps;

public class Npc
{
    public short Id { get; set; }                     // Unique NPC ID
    public string Name { get; set; } = string.Empty;  // NPC name for UI or logic reference

    public Position Position { get; set; }

    public short AvatarId { get; set; }               // Avatar or sprite ID
    public NpcState State { get; set; }               // Current NPC state (Idle, etc.)
    public bool IsInteractive { get; set; }           // Can the player interact with this NPC?
    public string[] Dialogues { get; set; } = [];     // Dialogues the NPC can say
}

public enum NpcState : byte
{
    Idle = 0,           // Đứng yên
    Sleeping = 1,       // Không hoạt động
    Hidden = 2,         // Không hiển thị trên map
}