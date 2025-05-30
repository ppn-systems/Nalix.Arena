using Nalix.Common.Connection;
using Nalix.Common.Connection.Protocols;
using Nalix.Common.Constants;
using Nalix.Common.Identity;
using Nalix.Common.Package;
using Nalix.Common.Package.Attributes;
using Nalix.Common.Package.Enums;
using Nalix.Common.Security;
using Nalix.Cryptography.Asymmetric;
using Nalix.Cryptography.Hashing;
using Nalix.Extensions.Primitives;
using Nalix.Logging;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Nalix.Game.Application.Operations;

/// <summary>
/// Quản lý quá trình bắt tay bảo mật để thiết lập kết nối mã hóa an toàn với client.
/// Sử dụng thuật toán X25519 để trao đổi khóa và SHA256 để băm, đảm bảo tính bảo mật và toàn vẹn.
/// Lớp này xử lý hai giai đoạn: khởi tạo và hoàn tất bắt tay, đồng thời duy trì trạng thái tạm thời
/// và dọn dẹp định kỳ để tối ưu hóa tài nguyên.
/// </summary>
[PacketController]
internal sealed class HandshakeOps<TPacket> where TPacket : IPacket, IPacketFactory<TPacket>
{
    // Lưu trữ trạng thái bắt tay tạm thời, sử dụng ConcurrentDictionary để đảm bảo an toàn luồng
    private static readonly ConcurrentDictionary<IEncodedId, HandshakeState> _states = new();

    // Thời gian chờ tối đa cho trạng thái bắt tay (10 giây)
    private static readonly System.TimeSpan HandshakeTimeout = System.TimeSpan.FromSeconds(10);

    // Khoảng thời gian giữa các lần dọn dẹp trạng thái hết hạn (1 phút)
    private static readonly System.TimeSpan CleanupInterval = System.TimeSpan.FromMinutes(1);

    /// <summary>
    /// Khởi tạo lớp HandshakeOps và kích hoạt tác vụ nền để dọn dẹp trạng thái bắt tay hết hạn.
    /// Tác vụ chạy liên tục, định kỳ xóa các trạng thái vượt quá thời gian chờ để tiết kiệm tài nguyên.
    /// </summary>
    static HandshakeOps()
    {
        // Khởi động tác vụ nền không đồng bộ để dọn dẹp trạng thái
        _ = Task.Run(CleanupLoop).ConfigureAwait(false);
    }

    /// <summary>
    /// Khởi tạo quá trình bắt tay bảo mật với client.
    /// Nhận gói tin chứa khóa công khai X25519 (32 byte), tạo cặp khóa X25519 cho server,
    /// tính toán khóa mã hóa chung, lưu trạng thái và gửi khóa công khai của server về client.
    /// Phương thức này kiểm tra định dạng gói tin và ngăn chặn tấn công lặp lại (replay attack).
    /// </summary>
    /// <param name="packet">Gói tin chứa khóa công khai X25519 của client (dự kiến 32 byte, định dạng nhị phân).</param>
    /// <param name="connection">Thông tin kết nối của client yêu cầu bắt tay.</param>
    /// <returns>Gói tin chứa khóa công khai của server hoặc thông báo lỗi nếu quá trình thất bại.</returns>
    [PacketEncryption(false)]
    [PacketTimeout(Timeouts.Moderate)]
    [PacketRateLimit(RequestLimitType.Low)]
    [PacketPermission(PermissionLevel.Guest)]
    [PacketOpcode((ushort)ProtocolCommand.StartHandshake)]
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    internal static System.Memory<byte> StartHandshake(IPacket packet, IConnection connection)
    {
        // Kiểm tra định dạng gói tin (phải là nhị phân để chứa khóa X25519)
        if (packet.Type != PacketType.Binary)
        {
            NLogix.Host.Instance.Debug("Received non-binary packet [Type={0}] from {1}",
                packet.Type, connection.RemoteEndPoint);
            return TPacket.Create((ushort)ProtocolCommand.StartHandshake, "Invalid packet type")
                          .Serialize();
        }

        // Xác thực độ dài khóa công khai (phải là 32 byte theo chuẩn X25519)
        if (packet.Payload.Length != 32)
        {
            NLogix.Host.Instance.Debug(
                "Invalid public key length [Length={0}] from {1}",
                packet.Payload.Length, connection.RemoteEndPoint);

            return TPacket.Create((ushort)ProtocolCommand.StartHandshake, ProtocolMessage.InvalidData)
                          .Serialize();
        }

        // Phát hiện và chặn nỗ lực tấn công lặp lại
        if (IsReplayAttempt(connection))
        {
            NLogix.Host.Instance.Debug(
                "Detected handshake replay attempt from {0}", connection.RemoteEndPoint);

            return TPacket.Create((ushort)ProtocolCommand.CompleteHandshake, ProtocolMessage.RateLimited)
                          .Serialize();
        }

        // Tạo cặp khóa X25519 (khóa riêng và công khai) cho server
        (byte[] privateKey, byte[] publicKey) = X25519.GenerateKeyPair();

        // Lưu trữ trạng thái bắt tay, bao gồm khóa riêng và thời gian khởi tạo
        var state = new HandshakeState
        {
            PrivateKey = privateKey,
            LastTime = System.DateTime.UtcNow
        };
        _states[connection.Id] = state;

        // Lấy khóa công khai của client từ gói tin
        byte[] clientPubKey = System.Runtime.InteropServices.MemoryMarshal.TryGetArray(
            packet.Payload, out System.ArraySegment<byte> seg)
            ? seg.Array : packet.Payload.ToArray();

        // Tính toán khóa mã hóa chung từ khóa riêng của server và khóa công khai của client
        connection.EncryptionKey = DeriveSharedKey(privateKey, clientPubKey);

        // Nâng cấp quyền truy cập của client lên mức User sau khi khởi tạo thành công
        connection.Level = PermissionLevel.User;

        // Gửi khóa công khai của server về client để tiếp tục giai đoạn hoàn tất
        return TPacket.Create(
            (ushort)ProtocolCommand.StartHandshake, PacketType.Binary,
            PacketFlags.None, PacketPriority.Low, publicKey).Serialize();
    }

    /// <summary>
    /// Hoàn tất quá trình bắt tay bảo mật bằng cách xác minh khóa công khai của client.
    /// Tính toán lại khóa mã hóa chung và so sánh với khóa hiện tại để đảm bảo tính toàn vẹn.
    /// Nếu thành công, kết nối bảo mật được thiết lập và trạng thái tạm thời được xóa.
    /// </summary>
    /// <param name="packet">Gói tin chứa khóa công khai của client để hoàn tất (dự kiến 32 byte).</param>
    /// <param name="connection">Thông tin kết nối của client.</param>
    /// <returns>Gói tin thông báo kết quả (thành công hoặc lỗi) của quá trình hoàn tất.</returns>
    [PacketEncryption(false)]
    [PacketTimeout(Timeouts.Moderate)]
    [PacketRateLimit(RequestLimitType.Low)]
    [PacketPermission(PermissionLevel.Guest)]
    [PacketOpcode((ushort)ProtocolCommand.CompleteHandshake)]
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    internal static System.Memory<byte> CompleteHandshake(IPacket packet, IConnection connection)
    {
        // Kiểm tra định dạng gói tin (phải là nhị phân)
        if (packet.Type != PacketType.Binary)
        {
            NLogix.Host.Instance.Debug(
                "Received non-binary packet [Type={0}] from {1}",
                packet.Type, connection.RemoteEndPoint);

            return TPacket.Create((ushort)ProtocolCommand.CompleteHandshake, ProtocolMessage.InvalidData)
                          .Serialize();
        }

        // Xác thực độ dài khóa công khai (phải là 32 byte)
        if (packet.Payload.Length != 32)
        {
            NLogix.Host.Instance.Debug(
                "Invalid public key length [Length={0}] from {1}",
                packet.Payload.Length, connection.RemoteEndPoint);

            return TPacket.Create((ushort)ProtocolCommand.CompleteHandshake, ProtocolMessage.InvalidPayload)
                          .Serialize();
        }

        // Lấy và xóa trạng thái bắt tay từ bộ nhớ tạm
        if (!_states.TryRemove(connection.Id, out var state))
        {
            NLogix.Host.Instance.Debug(
                "Missing handshake state for {0}", connection.RemoteEndPoint);

            return TPacket.Create((ushort)ProtocolCommand.CompleteHandshake, ProtocolMessage.UnknownError)
                          .Serialize();
        }

        // Tính toán lại khóa chung từ khóa riêng của server và khóa công khai của client
        byte[] derived = DeriveSharedKey(state.PrivateKey, packet.Payload.ToArray());
        System.Array.Clear(state.PrivateKey, 0, state.PrivateKey.Length); // Xóa khóa riêng để tăng bảo mật

        // So sánh khóa chung với khóa mã hóa hiện tại của kết nối
        if (connection.EncryptionKey is null || !connection.EncryptionKey.IsEqualTo(derived))
        {
            NLogix.Host.Instance.Debug(
                "Key mismatch during handshake finalization for {0}", connection.RemoteEndPoint);

            return TPacket.Create((ushort)ProtocolCommand.CompleteHandshake, ProtocolMessage.Conflict)
                          .Serialize();
        }

        // Ghi log khi kết nối bảo mật được thiết lập thành công
        NLogix.Host.Instance.Debug("Secure connection established for {0}", connection.RemoteEndPoint);
        return TPacket.Create((ushort)ProtocolCommand.CompleteHandshake, ProtocolMessage.Success)
                      .Serialize();
    }

    #region Private Methods

    /// <summary>
    /// Vòng lặp dọn dẹp các trạng thái bắt tay đã hết hạn.
    /// Định kỳ kiểm tra và xóa các trạng thái vượt quá thời gian chờ (HandshakeTimeout).
    /// </summary>
    /// <returns>Task đại diện cho vòng lặp dọn dẹp liên tục.</returns>
    private static async Task CleanupLoop()
    {
        while (true)
        {
            try
            {
                System.DateTime now = System.DateTime.UtcNow;
                foreach (var kvp in _states)
                {
                    if ((now - kvp.Value.LastTime) > HandshakeTimeout)
                        _states.TryRemove(kvp.Key, out _);
                }
            }
            catch (System.Exception ex)
            {
                NLogix.Host.Instance.Warn("Error during HandshakeOps cleanup: {0}", ex.Message);
            }

            await Task.Delay(CleanupInterval);
        }
    }

    /// <summary>
    /// Kiểm tra xem kết nối có đang cố gắng lặp lại bắt tay (replay attack) hay không.
    /// Trả về true nếu trạng thái bắt tay tồn tại và chưa hết thời gian chờ.
    /// </summary>
    /// <param name="connection">Thông tin kết nối của client.</param>
    /// <returns>True nếu phát hiện nỗ lực lặp lại, ngược lại là False.</returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static bool IsReplayAttempt(IConnection connection)
        => _states.TryGetValue(connection.Id, out HandshakeState state)
        && (System.DateTime.UtcNow - state.LastTime) < HandshakeTimeout;

    /// <summary>
    /// Tính toán khóa mã hóa chung bằng cách thực hiện trao đổi khóa X25519 và băm kết quả bằng SHA256.
    /// Kết hợp khóa riêng của server và khóa công khai của client để tạo bí mật chung,
    /// sau đó băm để tạo khóa mã hóa cuối cùng.
    /// </summary>
    /// <param name="privateKey">Khóa riêng của server dùng trong trao đổi khóa.</param>
    /// <param name="publicKey">Khóa công khai của client dùng trong trao đổi khóa.</param>
    /// <returns>Khóa mã hóa chung dùng để thiết lập kết nối bảo mật.</returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static byte[] DeriveSharedKey(byte[] privateKey, byte[] publicKey)
    {
        // Thực hiện trao đổi khóa X25519 để tạo bí mật chung
        byte[] secret = X25519.ComputeSharedSecret(privateKey, publicKey);

        // Băm bí mật chung bằng SHA256 để tạo khóa mã hóa
        return SHA256.HashData(secret);
    }

    /// <summary>
    /// Lớp nội bộ lưu trữ trạng thái bắt tay, bao gồm khóa riêng và thời gian khởi tạo.
    /// Được sử dụng để quản lý thông tin tạm thời trong quá trình bắt tay.
    /// </summary>
    private sealed class HandshakeState
    {
        public byte[] PrivateKey = null!;
        public System.DateTime LastTime;
    }

    #endregion Private Methods
}