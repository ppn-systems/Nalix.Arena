using Nalix.Game.Host.Terminals;
using Nalix.Host.Terminals;

namespace Nalix.Host;

internal class Program
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
    private static void Main(System.String[] args)
    {
        // Khởi tạo cấu hình ứng dụng, bao gồm máy chủ và cơ sở dữ liệu
        Terminal terminal = new(new ConsoleReader(), new ShortcutManager());

        terminal.ExitEvent.Wait(); // Chờ cho đến khi có yêu cầu thoát từ Terminal
    }
}