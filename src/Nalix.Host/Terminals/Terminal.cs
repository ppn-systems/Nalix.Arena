using Nalix.Logging;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nalix.Host.Terminals;

/// <summary>
/// Lớp Terminal chịu trách nhiệm quản lý giao tiếp với Console trong ứng dụng Server.
/// Bao gồm việc khởi tạo console, lắng nghe và xử lý các phím tắt để điều khiển trạng thái server.
/// </summary>
internal sealed class Terminal
{
    // Khóa để đảm bảo đọc phím an toàn khi nhiều luồng truy cập
    private static readonly Lock _keyReadLock = new();

    // Token hủy cho các tác vụ async
    private static readonly CancellationTokenSource _cTokenSrc = new();

    // Sự kiện dùng để báo hiệu thoát chương trình
    public readonly ManualResetEventSlim ExitEvent = new(false);

    // Interface đọc phím, dễ mock cho test
    private readonly IConsoleReader _consoleReader;

    // Quản lý danh sách phím tắt, mô tả và hành động tương ứng
    private readonly ShortcutManager _shortcutManager;

    /// <summary>
    /// Khởi tạo Terminal với interface đọc phím và quản lý phím tắt.
    /// </summary>
    /// <param name="consoleReader">Đối tượng đọc phím từ console</param>
    /// <param name="shortcutManager">Quản lý phím tắt và hành động</param>
    public Terminal(IConsoleReader consoleReader, ShortcutManager shortcutManager)
    {
        _consoleReader = consoleReader ?? throw new ArgumentNullException(nameof(consoleReader));
        _shortcutManager = shortcutManager ?? throw new ArgumentNullException(nameof(shortcutManager));

        InitializeConsole();
        RegisterDefaultShortcuts();

        // Khởi chạy vòng lặp sự kiện xử lý phím không đồng bộ
        _ = Task.Run(EventLoop);
    }

    /// <summary>
    /// Thiết lập các thuộc tính và sự kiện mặc định cho Console
    /// </summary>
    private void InitializeConsole()
    {
        Console.CursorVisible = false;       // Ẩn con trỏ chuột
        Console.TreatControlCAsInput = false; // Ctrl+C không bị dừng chương trình
        Console.OutputEncoding = Encoding.UTF8;
        Console.Title = $"Auto ({AppConfig.VersionBanner})"; // Đặt tiêu đề cửa sổ console

        // Bắt sự kiện Ctrl+C để cảnh báo, không cho phép thoát
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            NLogix.Host.Instance.Warn("Ctrl+C is disabled. Use Ctrl+H to show shortcuts.");
        };

        // Thiết lập sự kiện thoát và ngoại lệ không bắt được (unhandled)
        AppDomain.CurrentDomain.ProcessExit += (s, e) => ExitEvent.Set();
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            NLogix.Host.Instance.Error("Unhandled exception: " + e.ExceptionObject);
            ExitEvent.Set();
        };

        Console.Clear();
        Console.ResetColor();
        NLogix.Host.Instance.Info("Terminal initialized successfully.");
    }

    /// <summary>
    /// Đăng ký các phím tắt mặc định cùng hành động và mô tả của chúng
    /// </summary>
    private void RegisterDefaultShortcuts()
    {
        _shortcutManager.AddOrUpdateShortcut(ConsoleKey.H, ShowShortcuts, "Show shortcuts");

        _shortcutManager.AddOrUpdateShortcut(ConsoleKey.Q, () =>
        {
            NLogix.Host.Instance.Info("Ctrl+Q pressed: Initiating graceful shutdown...");
            if (AppConfig.Listener != null)
            {
                try
                {
                    if (AppConfig.Listener.IsListening)
                    {
                        _ = AppConfig.Listener.DeactivateAsync();
                        NLogix.Host.Instance.Info("Server stopped.");
                    }
                }
                catch (Exception ex)
                {
                    NLogix.Host.Instance.Error($"Error during shutdown: {ex.Message}");
                }
            }
            _cTokenSrc.Cancel();
            ExitEvent.Set();
            Environment.Exit(0);
        }, "Exit");

        _shortcutManager.AddOrUpdateShortcut(ConsoleKey.R, () =>
        {
            if (AppConfig.Listener == null)
            {
                NLogix.Host.Instance.Warn("Server is not initialized.");
                return;
            }
            _ = ThreadPool.QueueUserWorkItem(_ => AppConfig.Listener.ActivateAsync(_cTokenSrc.Token));
        }, "Run server");

        _shortcutManager.AddOrUpdateShortcut(ConsoleKey.P, () =>
        {
            if (AppConfig.Listener == null)
            {
                NLogix.Host.Instance.Warn("Server is not initialized.");
                return;
            }
            _ = Task.Run(() => AppConfig.Listener.DeactivateAsync());
        }, "Stop server");
    }

    /// <summary>
    /// Vòng lặp không đồng bộ để liên tục đọc phím từ console và xử lý phím tắt.
    /// Sử dụng khóa để đảm bảo thread-safe khi đọc phím và gọi hành động.
    /// </summary>
    /// <returns>Task bất đồng bộ</returns>
    private async Task EventLoop()
    {
        while (!ExitEvent.IsSet)
        {
            if (_consoleReader.KeyAvailable)
            {
                ConsoleKeyInfo keyInfo;
                lock (_keyReadLock) // tránh đọc đồng thời gây lỗi
                {
                    keyInfo = _consoleReader.ReadKey(true);
                    _ = _shortcutManager.TryExecuteShortcut(keyInfo.Modifiers, keyInfo.Key);
                }
            }
            await Task.Delay(10);
        }
    }

    /// <summary>
    /// Hiển thị danh sách các phím tắt hiện có cùng mô tả để người dùng tham khảo.
    /// </summary>
    private void ShowShortcuts()
    {
        String indent = new(' ', 10);
        StringBuilder builder = new();
        _ = builder.AppendLine("Available shortcuts:");
        foreach (var (key, description) in _shortcutManager.GetAllShortcuts())
        {
            _ = builder.AppendLine($"{indent}Ctrl+{key}".PadRight(15) + $"→ {description}");
        }
        NLogix.Host.Instance.Info(builder.ToString());
    }

    /// <summary>
    /// Cho phép đăng ký hoặc cập nhật phím tắt với hành động và mô tả cụ thể.
    /// </summary>
    /// <param name="key">Phím console (ConsoleKey)</param>
    /// <param name="action">Hành động thực thi khi nhấn phím</param>
    /// <param name="description">Mô tả chức năng phím tắt</param>
    public void SetShortcut(ConsoleKey key, Action action, String description)
        => _shortcutManager.AddOrUpdateShortcut(key, action, description);
}