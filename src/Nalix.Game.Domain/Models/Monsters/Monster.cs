using Nalix.Game.Domain.Common;
using Nalix.Game.Domain.Entities;
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
    public bool IsAlive => Stats.Health > 0;

    /// <summary>
    /// Bảng vật phẩm rơi ra khi quái vật bị tiêu diệt.
    /// </summary>
    public LootTable Loot { get; set; }

    /// <summary>
    /// Loại quái vật (ví dụ: cận chiến, tầm xa...).
    /// </summary>
    public MonsterType Type { get; set; }

    /// <summary>
    /// Vị trí hiện tại của quái vật trên bản đồ.
    /// </summary>
    public Position Position { get; set; }

    /// <summary>
    /// Các chỉ số của quái vật (máu, sát thương, phòng thủ...).
    /// </summary>
    public MonsterStats Stats { get; set; }

    /// <summary>
    /// Thông tin về thời gian và cách làm mới quái vật trên bản đồ.
    /// </summary>
    public RefreshInfo Refresh { get; set; }

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
        Id = 0;
        TimeAttack = 10000 + Clock.UnixMillisecondsNow();

        Position = new Position(0, 0);
        Stats = new MonsterStats();
        Loot = new LootTable();
        Refresh = new RefreshInfo();
        Type = MonsterType.Melee;
    }
}