namespace Nalix.Game.Domain.Models.Tasks;

/// <summary>
/// Phân loại các loại nhiệm vụ trong game.
/// </summary>
public enum TaskType : byte
{
    /// <summary>
    /// Giết một hoặc nhiều quái vật cụ thể.
    /// </summary>
    KillMonster = 0,

    /// <summary>
    /// Thu thập một số lượng vật phẩm nhất định.
    /// </summary>
    CollectItem = 1,

    /// <summary>
    /// Nói chuyện với NPC cụ thể.
    /// </summary>
    TalkToNpc = 2,

    /// <summary>
    /// Đến một khu vực nhất định (kích hoạt khi đến đó).
    /// </summary>
    ReachLocation = 3,

    /// <summary>
    /// Sử dụng một vật phẩm cụ thể.
    /// </summary>
    UseItem = 4,

    /// <summary>
    /// Thực hiện hành động đặc biệt.
    /// </summary>
    PerformAction = 5,

    /// <summary>
    /// Nhiệm vụ dạng sự kiện đặc biệt.
    /// </summary>
    Event = 6
}