using Nalix.Common.Connection;
using Nalix.Common.Constants;
using Nalix.Common.Package;
using Nalix.Common.Package.Attributes;
using Nalix.Common.Package.Enums;
using Nalix.Common.Security.Types;
using Nalix.Cryptography.Asymmetric;
using Nalix.Cryptography.Hashing;
using Nalix.Game.Application.Caching;
using Nalix.Game.Shared.Commands;
using Nalix.Logging;

namespace Nalix.Game.Application.Operations;

/// <summary>
/// Quản lý quá trình bắt tay bảo mật để thiết lập kết nối mã hóa an toàn với client.
/// Sử dụng thuật toán trao đổi khóa X25519 và băm SHA256 để đảm bảo tính bảo mật và toàn vẹn của kết nối.
/// Lớp này chịu trách nhiệm khởi tạo bắt tay, tạo cặp khóa, và tính toán khóa mã hóa chung.
/// </summary>
[PacketController]
internal sealed class HandshakeOps<TPacket> where TPacket : IPacket, IPacketFactory<TPacket>
{
    /// <summary>
    /// Khởi tạo quá trình bắt tay bảo mật với client.
    /// Nhận gói tin chứa khóa công khai X25519 (32 byte) từ client, tạo cặp khóa X25519 cho server,
    /// tính toán khóa mã hóa chung, và gửi khóa công khai của server về client.
    /// Phương thức này kiểm tra định dạng gói tin để đảm bảo an toàn và hiệu quả.
    /// </summary>
    /// <param name="packet">Gói tin chứa khóa công khai X25519 của client, yêu cầu định dạng nhị phân và độ dài 32 byte.</param>
    /// <param name="connection">Thông tin kết nối của client yêu cầu bắt tay bảo mật.</param>
    /// <returns>Gói tin chứa khóa công khai của server hoặc thông báo lỗi nếu quá trình thất bại.</returns>
    [PacketEncryption(false)]
    [PacketTimeout(Timeouts.Moderate)]
    [PacketRateLimit(RequestLimitType.Low)]
    [PacketPermission(PermissionLevel.Guest)]
    [PacketOpcode((System.UInt16)Command.Handshake)]
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    internal static System.Memory<System.Byte> Handshake(IPacket packet, IConnection connection)
    {
        // Nếu đã handshake, không cho phép lặp lại
        if (connection.EncryptionKey is not null)
        {
            NLogix.Host.Instance.Warn(
                "Handshake already completed for {0}",
                connection.RemoteEndPoint);

            return PacketCache<TPacket>.HandshakeAlreadyDone;
        }

        // Kiểm tra định dạng gói tin, đảm bảo là nhị phân để chứa khóa công khai X25519
        if (packet.Type != PacketType.Binary)
        {
            NLogix.Host.Instance.Debug(
                "Received non-binary packet [Type={0}] from {1}",
                packet.Type, connection.RemoteEndPoint);

            return PacketCache<TPacket>.HandshakeInvalidType;
        }

        // Xác thực độ dài khóa công khai, phải đúng 32 byte theo chuẩn X25519
        if (packet.Payload.Length != 32)
        {
            NLogix.Host.Instance.Debug(
                "Invalid public key length [Length={0}] from {1}",
                packet.Payload.Length, connection.RemoteEndPoint);

            return PacketCache<TPacket>.HandshakeInvalidKeyLength;
        }

        // Tạo cặp khóa X25519 (khóa riêng và công khai) cho server
        X25519.GenerateKeyPair(out System.Byte[] privateKey, out System.Byte[] publicKey);

        // Thực hiện trao đổi khóa X25519 để tạo bí mật chung
        // Kết hợp khóa riêng của server và khóa công khai của client để tạo bí mật chung
        System.Span<System.Byte> secret = stackalloc System.Byte[32];
        X25519.ComputeSharedSecret(privateKey, packet.Payload.Span, secret);

        // Băm bí mật chung bằng SHA256 để tạo khóa mã hóa an toàn
        connection.EncryptionKey = SHA256.HashData(secret);

        System.Array.Clear(privateKey, 0, privateKey.Length);

        // Nâng cấp quyền truy cập của client lên mức User
        connection.Level = PermissionLevel.User;

        // Gửi khóa công khai của server về client để tiếp tục giai đoạn bắt tay
        return TPacket.Create(
            Command.Handshake.AsUInt16(), PacketType.Binary,
            PacketFlags.None, PacketPriority.Low, publicKey).Serialize();
    }
}