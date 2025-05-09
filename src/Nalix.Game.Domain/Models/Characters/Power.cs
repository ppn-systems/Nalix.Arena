namespace Nalix.Game.Domain.Models.Characters;

public class Power
{
    public long Strength { get; set; }       // Sức mạnh (Strength)
    public long Potential { get; set; }      // Tiềm năng (Potential)

    public long Attack { get; set; }         // Sức tấn công (Damage)
    public long Health { get; set; }         // Máu tối đa (HP)
    public long Energy { get; set; }         // Năng lượng tối đa (Energy)
    public int Defense { get; set; }         // Phòng thủ
    public byte CriticalRate { get; set; }   // Tỉ lệ chí mạng (%)
}