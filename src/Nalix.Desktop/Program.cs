using Nalix.Rendering.Runtime;

namespace Nalix.Desktop;

/// <summary>
/// Điểm khởi đầu của trò chơi. Khởi chạy cửa sổ trò chơi thông qua <see cref="GameEngine"/>.
/// </summary>
internal static class Program
{
    /// <summary>
    /// Phương thức chính khởi chạy ứng dụng. Gọi <see cref="GameEngine.OpenWindow"/> để mở cửa sổ trò chơi.
    /// </summary>
    /// <param name="args">Các đối số dòng lệnh (chưa được sử dụng).</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("Calls Nalix.Client.Registry.Load()")]
    public static void Main(System.String[] args)
    {
        // Initialize client-side services and registrations.
        Nalix.Communication.Registry.Load();

        // Open the game window.
        GameEngine.OpenWindow();
    }
}