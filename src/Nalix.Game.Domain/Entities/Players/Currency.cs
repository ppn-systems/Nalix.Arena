namespace Nalix.Game.Domain.Entities.Players;

/// <summary>
/// Lớp chứa thông tin về tiền tệ của người chơi.
/// </summary>
public sealed class Currency
{
    /// <summary>
    /// Số lượng vàng của người chơi.
    /// </summary>
    public uint Gold { get; set; }

    /// <summary>
    /// Số lượng đá quý của người chơi.
    /// </summary>
    public uint Gems { get; set; }
}