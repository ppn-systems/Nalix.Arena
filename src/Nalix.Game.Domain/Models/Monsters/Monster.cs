using Nalix.Game.Domain.Common;
using Nalix.Game.Domain.Entities;
using Nalix.Game.Domain.Interface;
using Nalix.Game.Domain.Models.Attacks;
using Nalix.Game.Domain.Models.Combat;
using Nalix.Shared.Time;
using System.Collections.Generic;

namespace Nalix.Game.Domain.Models.Monsters;

/// <summary>
/// Lớp trừu tượng đại diện cho một quái vật trong game, kế thừa từ <see cref="NamedEntity{TId}"/> và triển khai giao diện <see cref="IMonster"/>.
/// </summary>
public abstract class Monster : NamedEntity<uint>, IMonster
{
    /// <summary>
    /// Thời điểm tấn công tiếp theo của quái vật (dựa trên Unix milliseconds).
    /// </summary>
    public long TimeAttack { get; set; }

    /// <summary>
    /// Kiểm tra xem quái vật còn sống hay không (dựa trên chỉ số máu).
    /// </summary>
    public bool IsAlive => CombatStats.Health > 0;

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
    public CharacterStats CombatStats { get; set; }

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
    public Dictionary<int, long> PlayerAttack { get; set; }

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

    public void TakeDamage(long amount)
    {
        // Giảm sát thương theo giáp (Armor)
        long damageTaken = amount - CombatStats.Defense;
        if (damageTaken < 1)
            damageTaken = 1; // Luôn nhận ít nhất 1 sát thương

        CombatStats.Health -= damageTaken;

        if (CombatStats.Health < 0)
            CombatStats.Health = 0;
    }

    public long CalculateDamage(ICombatant target)
    {
        // Nếu target có chỉ số phòng thủ
        long targetArmor = 0;
        if (target is ICombatant combatant)
        {
            targetArmor = combatant.CombatStats.Defense;
        }

        long finalDamage = CombatStats.Attack - targetArmor;
        return finalDamage < 1 ? 1 : finalDamage; // Tối thiểu luôn gây 1 sát thương
    }
}