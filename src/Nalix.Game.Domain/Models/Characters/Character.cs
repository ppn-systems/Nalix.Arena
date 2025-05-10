using Nalix.Game.Domain.Models.Items;
using Nalix.Game.Domain.Shared;

namespace Nalix.Game.Domain.Models.Characters;

public class Character
{
    public int MapId { get; set; }

    public Power Power { get; set; }
    public Position Position { get; set; }

    // Inventory and Chest for the player

    public Currency Currency { get; set; }         // Tiền tệ của người chơi
    public ItemContainer Chest { get; set; }
    public ItemContainer Inventory { get; set; }
}