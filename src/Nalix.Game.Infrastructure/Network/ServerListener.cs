using Nalix.Common.Caching;
using Nalix.Common.Logging;
using Nalix.Network.Listeners;
using Nalix.Network.Protocols;

namespace Nalix.Game.Infrastructure.Network;

/// <summary>
/// Lớp `ServerListener` quản lý việc lắng nghe các kết nối mạng.
/// </summary>
/// <remarks>
/// Được kế thừa từ `Listener`, lớp này cung cấp cơ chế cập nhật thời gian cho các sự kiện mạng.
/// </remarks>
/// <param name="protocol">Giao thức mạng được sử dụng.</param>
/// <param name="bufferPool">Bộ nhớ đệm để quản lý dữ liệu mạng.</param>
/// <param name="logger">Trình ghi log cho các sự kiện hệ thống.</param>
public sealed class ServerListener(IProtocol protocol, IBufferPool bufferPool, ILogger logger)
    : Listener(protocol, bufferPool, logger)
{
    /// <summary>
    /// Cập nhật thời gian hệ thống dựa trên số mili-giây đã trôi qua.
    /// </summary>
    /// <param name="milliseconds">Số mili-giây cần cập nhật.</param>
    public override void UpdateTime(long milliseconds)
    {
    }
}