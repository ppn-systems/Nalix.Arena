using Nalix.Rendering.Resources;

namespace Nalix.Presentation;

/// <summary>
/// Cung cấp quyền truy cập tập trung đến các bộ tải tài nguyên được sử dụng trong lớp trình bày của trò chơi.
/// </summary>
internal static class Assets
{
    /// <summary>
    /// Tải các phông chữ từ thư mục <c>assets/fonts</c>.
    /// </summary>
    public static readonly FontLoader Font = new("assets/fonts");

    /// <summary>
    /// Tải các hiệu ứng âm thanh từ thư mục <c>assets/sounds</c>.
    /// </summary>
    public static readonly SfxLoader Sounds = new("assets/sounds");

    /// <summary>
    /// Tải các kết cấu giao diện người dùng (UI) từ thư mục <c>assets/ui</c>.
    /// </summary>
    public static readonly TextureLoader UiTextures = new("assets/ui");
}