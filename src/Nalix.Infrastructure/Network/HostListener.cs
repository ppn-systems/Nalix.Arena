using Nalix.Network.Abstractions;
using Nalix.Network.Listeners.Tcp;

namespace Nalix.Infrastructure.Network;

/// <summary>
/// Lớp `HostListener` quản lý việc lắng nghe các kết nối mạng.
/// </summary>
public sealed class HostListener : TcpListenerBase
{
    /// <summary>
    /// Được kế thừa từ `Listener`, lớp này cung cấp cơ chế cập nhật thời gian cho các sự kiện mạng.
    /// </summary>
    /// <param name="protocol">Giao thức mạng được sử dụng.</param>
    public HostListener(IProtocol protocol)
        : base(protocol) => IsTimeSyncEnabled = false; // Bật đồng bộ thời gian

    /// <summary>
    /// Cập nhật thời gian hệ thống dựa trên số mili-giây đã trôi qua.
    /// </summary>
    /// <param name="milliseconds">Số mili-giây cần cập nhật.</param>
    public override void SynchronizeTime(System.Int64 milliseconds)
    {
    }
}