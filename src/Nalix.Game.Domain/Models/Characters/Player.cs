using Nalix.Common.Connection;

namespace Nalix.Game.Domain.Models.Characters;

public sealed class Player
{
    public int Id { get; set; }
    public string Name { get; set; }

    public Character Character { get; set; }
    public IConnection Connection { get; set; }     // Kết nối của người chơi
}