namespace Nalix.Game.Presentation.Scenes;

/// <summary>
/// Chứa tên của các cảnh (scene) trong trò chơi để dễ dàng quản lý.
/// </summary>
internal static class SceneNames
{
    /// <summary>
    /// Cảnh chính của trò chơi (menu hoặc màn chơi chính).
    /// </summary>
    public const string Main = "main";

    /// <summary>
    /// Cảnh kết nối mạng (hiển thị khi đang cố gắng kết nối đến server).
    /// </summary>
    public const string Network = "network";

    /// <summary>
    /// Cảnh thiết lập trò chơi (âm thanh, hình ảnh, v.v.).
    /// </summary>
    public const string Settings = "settings";
}