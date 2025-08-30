using Nalix.Common.Connection;
using Nalix.Common.Packets.Abstractions;
using Nalix.Common.Packets.Attributes;
using Nalix.Common.Security.Types;
using Nalix.Communication.Commands;
using Nalix.Cryptography.Asymmetric;
using Nalix.Cryptography.Hashing;
using Nalix.Logging;
using Nalix.Shared.Injection;
using Nalix.Shared.Memory.Pooling;
using Nalix.Shared.Messaging.Control;
using Nalix.Shared.Messaging.Text;

namespace Nalix.Application.Operations;

/// <summary>
/// Quản lý quá trình bắt tay bảo mật để thiết lập kết nối mã hóa an toàn với client.
/// Sử dụng thuật toán trao đổi khóa X25519 và băm SHA256 để đảm bảo tính bảo mật và toàn vẹn của kết nối.
/// Lớp này chịu trách nhiệm khởi tạo bắt tay, tạo cặp khóa, và tính toán khóa mã hóa chung.
/// </summary>
[PacketController]
internal sealed class HandshakeOps
{
    static HandshakeOps()
    {
        _ = InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>()
                                .SetMaxCapacity<Handshake>(1024);

        _ = InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>()
                                .Prealloc<Handshake>(512);
    }

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
    [PacketPermission(PermissionLevel.Guest)]
    [PacketOpcode((System.UInt16)Command.Handshake)]
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static System.Memory<System.Byte> Handshake(
        IPacket packet,
        IConnection connection)
    {
        if (packet is not Handshake initPacket)
        {
            NLogix.Host.Instance.Error(
                "Invalid packet type. Expected HandshakePacket from {0}",
                connection.RemoteEndPoint);

            return CreateResponse("Invalid packet type");
        }

        // Nếu đã handshake, không cho phép lặp lại - theo security best practices
        if (connection.EncryptionKey is not null)
        {
            NLogix.Host.Instance.Warn(
                "Handshake already completed for {0}",
                connection.RemoteEndPoint);

            return CreateResponse("Handshake already completed");
        }

        // Defensive programming - kiểm tra payload null
        if (initPacket.Data is null)
        {
            NLogix.Host.Instance.Error(
                "Null payload in handshake packet from {0}",
                connection.RemoteEndPoint);

            return CreateResponse("Invalid payload");
        }

        // Xác thực độ dài khóa công khai, phải đúng 32 byte theo chuẩn X25519
        if (initPacket.Data.Length != 32)
        {
            NLogix.Host.Instance.Debug(
                "Invalid public key length [Length={0}] from {1}",
                initPacket.Data.Length, connection.RemoteEndPoint);

            return CreateResponse($"Invalid key length: expected 32, got {initPacket.Data.Length}");
        }

        // Tạo response packet chứa public key của server
        Handshake response = InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>()
                                                     .Get<Handshake>();

        try
        {
            // Tạo cặp khóa X25519 (khóa riêng và công khai) cho server
            X25519.X25519KeyPair keyPair = X25519.GenerateKeyPair();

            // Tính toán shared secret từ private key của server và public key của client
            System.Byte[] secret = X25519.Agreement(keyPair.PrivateKey, initPacket.Data);

            // Băm bí mật chung bằng SHA256 để tạo khóa mã hóa an toàn
            connection.EncryptionKey = SHA256.HashData(secret);

            // Security: Clear sensitive data từ memory
            System.Array.Clear(keyPair.PrivateKey, 0, keyPair.PrivateKey.Length);
            System.Array.Clear(secret, 0, secret.Length);

            // Nâng cấp quyền truy cập của client lên mức User
            connection.Level = PermissionLevel.User;

            // Log successful handshake
            NLogix.Host.Instance.Info(
                "Handshake completed successfully for {0}",
                connection.RemoteEndPoint);

            response.Initialize(keyPair.PublicKey);

            return new System.Memory<System.Byte>(response.Serialize());
        }
        catch (System.Exception ex)
        {
            // Error handling theo security best practices
            NLogix.Host.Instance.Error(
                "Handshake failed for {0}: {1}",
                connection.RemoteEndPoint, ex.Message);

            // Reset connection state nếu có lỗi
            connection.EncryptionKey = null;
            connection.Level = PermissionLevel.Guest;

            return CreateResponse("Handshake processing failed");
        }
        finally
        {
            InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>()
                                    .Return<Handshake>(response);
        }
    }

    /// <summary>
    /// Tạo success response packet sử dụng StringPacket
    /// Theo Single Responsibility Principle - chỉ tập trung vào việc tạo response
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static System.Memory<System.Byte> CreateResponse(System.String message)
    {
        Text256 response = InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>()
                                                 .Get<Text256>();

        try
        {
            response.Initialize(message);

            return new System.Memory<System.Byte>(response.Serialize());
        }
        finally
        {
            // Đảm bảo trả lại packet về pool sau khi sử dụng
            InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>()
                                    .Return<Text256>(response);
        }
    }
}