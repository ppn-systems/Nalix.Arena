using Nalix.Game.Domain.Shared;

namespace Nalix.Game.Domain.Models.Characters;

public class Player
{
    public int Id { get; set; }
    public int MapId { get; set; }
    public string Name { get; set; }

    public Power Power { get; set; }
    public Position Position { get; set; }
}