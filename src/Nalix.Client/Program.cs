using Nalix.Rendering.Runtime;

namespace Nalix.Client;

/// <summary>
/// Điểm khởi đầu của trò chơi. Khởi chạy cửa sổ trò chơi thông qua <see cref="GameEngine"/>.
/// </summary>
internal class Program
{
    /// <summary>
    /// Phương thức chính khởi chạy ứng dụng. Gọi <see cref="GameEngine.OpenWindow"/> để mở cửa sổ trò chơi.
    /// </summary>
    /// <param name="args">Các đối số dòng lệnh (chưa được sử dụng).</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
    public static void Main(System.String[] args) => GameEngine.OpenWindow();
}