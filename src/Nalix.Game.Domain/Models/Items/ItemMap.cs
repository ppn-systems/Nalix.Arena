using Nalix.Game.Domain.Shared;

namespace Nalix.Game.Domain.Models.Items;

public class ItemMap
{
    public int Id { get; set; }
    public int PlayerId { get; set; }

    public sbyte Radius { get; set; }
    public long LeftTime { get; set; }

    public Item Item { get; set; }
    public Position Position { get; set; }
}