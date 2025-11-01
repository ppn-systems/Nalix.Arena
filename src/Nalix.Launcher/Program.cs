// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Nalix.Communication;
using Nalix.Rendering.Runtime;

namespace Nalix.Launcher;

/// <summary>
/// Điểm khởi đầu của trò chơi. Khởi chạy cửa sổ trò chơi thông qua <see cref="GraphicsEngine"/>.
/// </summary>
internal static class Program
{
    /// <summary>
    /// Phương thức chính khởi chạy ứng dụng. Gọi <see cref="GraphicsEngine.OpenWindow"/> để mở cửa sổ trò chơi.
    /// </summary>
    /// <param name="args">Các đối số dòng lệnh (chưa được sử dụng).</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("Calls Nalix.Client.Registry.Load()")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Roslynator", "RCS1163:Unused parameter", Justification = "<Pending>")]
    public static void Main(System.String[] args)
    {
        // Initialize client-side services and registrations.
        Registry.Load();

        // Open the game window.
        GraphicsEngine.OpenWindow();
    }
}