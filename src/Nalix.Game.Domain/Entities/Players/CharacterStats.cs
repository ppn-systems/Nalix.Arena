namespace Nalix.Game.Domain.Entities.Players;

public sealed class CharacterStats
{
    public long Attack { get; set; }         // Sức tấn công (Damage)
    public long Health { get; set; }         // Máu tối đa (HP)
    public long Energy { get; set; }         // Năng lượng tối đa (Energy)
    public long Defense { get; set; }         // Phòng thủ
    public float CriticalRate { get; set; }   // Tỉ lệ chí mạng (%)
}