using Nalix.Game.Domain.Entities;
using Nalix.Game.Domain.Entities.Players;
using Nalix.Game.Domain.Models.Items;

namespace Nalix.Game.Domain.Models.Characters;

public sealed class Character
{
    public int MapId { get; set; }

    public Position Position { get; set; }
    public CharacterStats CharacterStats { get; set; }

    // Inventory and Chest for the player

    public Currency Currency { get; set; }         // Tiền tệ của người chơi
    public ItemContainer Chest { get; set; }
    public ItemContainer Inventory { get; set; }
}