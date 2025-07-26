using Nalix.Domain.Common;
using Nalix.Domain.Entities;
using Nalix.Domain.Interface;
using Nalix.Domain.Models.Attacks;
using Nalix.Domain.Models.Combat;
using Nalix.Framework.Time;
using System.Collections.Generic;

namespace Nalix.Domain.Models.Monsters;

/// <summary>
/// Lớp trừu tượng đại diện cho một quái vật trong game, kế thừa từ <see cref="NamedEntity{TId}"/> và
/// triển khai giao diện <see cref="IMonster"/>.
/// </summary>
public abstract class Monster : NamedEntity<System.UInt32>, IMonster
{
    /// <summary>
    /// Thời điểm tấn công tiếp theo của quái vật (dựa trên Unix milliseconds).
    /// </summary>
    public System.Int64 TimeAttack { get; set; }

    /// <summary>
    /// Kiểm tra xem quái vật còn sống hay không (dựa trên chỉ số máu).
    /// </summary>
    public System.Boolean IsAlive => CharacterStats.Health > 0;

    /// <summary>
    /// Bảng vật phẩm rơi ra khi quái vật bị tiêu diệt.
    /// </summary>
    public LootTable Loot { get; set; }

    /// <summary>
    /// Vị trí hiện tại của quái vật trên bản đồ.
    /// </summary>
    public Position Position { get; set; }

    /// <summary>
    /// Thông tin về thời gian và cách làm mới quái vật trên bản đồ.
    /// </summary>
    public RefreshInfo Refresh { get; set; }

    /// <summary>
    /// Thông tin về chỉ số chiến đấu của quái vật (sát thương, phòng thủ, tốc độ...).
    /// </summary>
    public CharacterStats CharacterStats { get; set; }

    /// <summary>
    /// Các chỉ số của quái vật (máu gốc, kinh nghiệm...).
    /// </summary>
    public MonsterStats MonsterStats { get; set; }

    /// <summary>
    /// Hành vi tấn công của quái vật, có thể là tấn công cận chiến hoặc tấn công từ xa.
    /// </summary>
    public IAttackBehavior MeleeAttack { get; set; }

    /// <summary>
    /// Hành vi tấn công từ xa của quái vật, có thể là tấn công bằng phép thuật hoặc vũ khí.
    /// </summary>
    public IAttackBehavior RangedAttack { get; set; }

    /// <summary>
    /// Danh sách lưu trữ thông tin về sát thương mà các người chơi gây ra cho quái vật
    /// key: Id người chơi, value: lượng sát thương.
    /// </summary>
    public Dictionary<System.Int32, System.Int64> PlayerAttack { get; set; }

    /// <summary>
    /// Khởi tạo một đối tượng quái vật với các giá trị mặc định.
    /// </summary>
    public Monster()
    {
        // Khởi tạo các giá trị mặc định nếu cần
        this.Id = 0;
        this.TimeAttack = 10000 + Clock.UnixMillisecondsNow();

        this.Position = new Position(0, 0);
        this.MonsterStats = new MonsterStats();
        this.Loot = new LootTable();
        this.Refresh = new RefreshInfo();
    }

    public void TakeDamage(System.Int64 amount)
    {
        // Giảm sát thương theo giáp (Armor)
        System.Int64 damageTaken = amount - CharacterStats.Defense;
        if (damageTaken < 1)
        {
            damageTaken = 1; // Luôn nhận ít nhất 1 sát thương
        }

        CharacterStats.Health -= damageTaken;

        if (CharacterStats.Health < 0)
        {
            CharacterStats.Health = 0;
        }
    }

    public System.Int64 CalculateDamage(ICombatant target)
    {
        // Nếu target có chỉ số phòng thủ
        System.Int64 targetArmor = 0;
        if (target is ICombatant combatant)
        {
            targetArmor = combatant.CharacterStats.Defense;
        }

        System.Int64 finalDamage = CharacterStats.Attack - targetArmor;
        return finalDamage < 1 ? 1 : finalDamage; // Tối thiểu luôn gây 1 sát thương
    }
}