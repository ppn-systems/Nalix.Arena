// Copyright (c) 2025 PPN Corporation. All rights reserved.

namespace Nalix.Desktop.Scenes;

/// <summary>
/// Chứa tên của các cảnh (scene) trong trò chơi để dễ dàng quản lý.
/// </summary>
internal static class SceneNames
{
    /// <summary>
    /// Cảnh chính của trò chơi (menu hoặc màn chơi chính).
    /// </summary>
    public const System.String Main = "main";

    /// <summary>
    /// Cảnh đăng nhập (hiển thị form đăng nhập).
    /// </summary>
    public const System.String Login = "login";

    /// <summary>
    /// Cảnh hiện thị thông tin về nhóm phát triển trò chơi.
    /// </summary>
    public const System.String Credits = "credits";

    /// <summary>
    /// Cảnh kết nối mạng (hiển thị khi đang cố gắng kết nối đến server).
    /// </summary>
    public const System.String Network = "network";

    /// <summary>
    /// Cảnh bắt tay (handshake) với server sau khi kết nối mạng thành công.
    /// </summary>
    public const System.String Handshake = "handshake";

    /// <summary>
    /// Cảnh thiết lập trò chơi (âm thanh, hình ảnh, v.v.).
    /// </summary>
    public const System.String Settings = "settings";
}