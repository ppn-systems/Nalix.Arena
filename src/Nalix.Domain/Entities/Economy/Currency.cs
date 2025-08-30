namespace Nalix.Domain.Entities.Economy;

/// <summary>
/// Lớp chứa thông tin về tiền tệ của người chơi.
/// </summary>
public sealed class Currency
{
    /// <summary>
    /// Số lượng vàng của người chơi.
    /// </summary>
    public System.UInt32 Gold { get; set; }

    /// <summary>
    /// Số lượng đá quý của người chơi.
    /// </summary>
    public System.UInt32 Gems { get; set; }
}