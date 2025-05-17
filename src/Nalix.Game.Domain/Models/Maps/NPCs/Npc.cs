using Nalix.Game.Domain.Entities;

namespace Nalix.Game.Domain.Models.Maps.NPCs;

public class Npc
{
    public short Id { get; set; }                     // Unique NPC ID
    public short AvatarId { get; set; }               // Avatar or sprite ID
    public string Name { get; set; } = string.Empty;  // NPC name for UI or logic reference

    public NpcType Type { get; set; }                 // NPC type (Merchant, Quest Giver, etc.)
    public NpcState State { get; set; }               // Current NPC state (Idle, etc.)
    public Position Position { get; set; }            // NPC position on the map
    public bool IsInteractive { get; set; }           // Can the player interact with this NPC?
    public string[] Dialogues { get; set; } = [];     // Dialogues the NPC can say
}