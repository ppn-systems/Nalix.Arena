// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Nalix.Rendering.Resources;
using Nalix.Rendering.Resources.Manager;
using Nalix.Rendering.Runtime;

namespace Nalix.Launcher;

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
    public static readonly SfxLoader SfxLoader = new("assets/sounds");

    /// <summary>
    /// Tải các kết cấu giao diện người dùng (UI) từ thư mục <c>assets/ui</c>.
    /// </summary>
    public static readonly TextureLoader UiTextures = new("assets/ui");

    /// <summary>
    /// Cung cấp quản lý hiệu ứng âm thanh với mức âm lượng được lấy từ cấu hình đồ họa của trò chơi.
    /// </summary>
    public static readonly SfxManager Sfx = new(SfxLoader, () => (System.Int32)GraphicsEngine.GraphicsConfig.SoundVolume);

    static Assets() => Sfx.AddToLibrary("1", 1);
}