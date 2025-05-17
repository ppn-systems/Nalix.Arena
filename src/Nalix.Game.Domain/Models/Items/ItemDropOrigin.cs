namespace Nalix.Game.Domain.Models.Items;

/// <summary>
/// Xác định nguồn gốc của vật phẩm khi được rơi.
/// </summary>
public enum ItemDropOrigin
{
    /// <summary>
    /// Không có nguồn gốc cụ thể.
    /// </summary>
    None,

    /// <summary>
    /// Rơi từ nhiệm vụ.
    /// </summary>
    Quest,

    /// <summary>
    /// Hệ thống tạo ra.
    /// </summary>
    System,

    /// <summary>
    /// Do người chơi tạo ra.
    /// </summary>
    Player,

    /// <summary>
    /// Rơi từ quái vật.
    /// </summary>
    Monster
}