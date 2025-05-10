namespace Nalix.Game.Domain.Models.Monsters;

public class MonsterStats
{
    public int Level { get; set; }               // Cấp độ của quái vật
    public int Armor { get; set; }               // Chỉ số phòng thủ
    public int Damage { get; set; }              // Sát thương
    public int Health { get; set; }              // Sức khỏe hiện tại
    public int MaxHealth { get; set; }           // Sức khỏe tối đa
    public int Experience { get; set; }          // Kinh nghiệm quái vật rơi
}