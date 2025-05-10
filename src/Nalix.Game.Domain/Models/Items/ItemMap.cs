using Nalix.Game.Domain.Shared;
using Nalix.Shared.Time;

namespace Nalix.Game.Domain.Models.Items;

public class ItemMap
{
    public int Id { get; set; }
    public int PlayerId { get; set; }

    public sbyte Radius { get; set; }
    public long LeftTime { get; set; }

    public bool IsExpired => Clock.UnixMillisecondsNow() > LeftTime;

    public Item Item { get; set; }
    public Position Position { get; set; }

    public ItemDropOrigin DropOrigin { get; set; } = ItemDropOrigin.None;
}