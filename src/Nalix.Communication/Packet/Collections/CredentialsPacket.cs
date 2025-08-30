using Nalix.Common.Connection.Protocols;
using Nalix.Common.Packets;
using Nalix.Common.Packets.Abstractions;
using Nalix.Common.Packets.Enums;
using Nalix.Common.Security.Enums;
using Nalix.Common.Serialization;
using Nalix.Common.Serialization.Attributes;
using Nalix.Communication.Commands;
using Nalix.Communication.Messages;
using Nalix.Communication.Security;
using Nalix.Cryptography;
using Nalix.Shared.Injection;
using Nalix.Shared.Memory.Pooling;
using Nalix.Shared.Serialization;
using System;

namespace Nalix.Communication.Packet.Collections;

/// <summary>
/// Gói tin chứa thông tin đăng nhập từ client (username, mật khẩu băm, metadata),
/// dùng trong quá trình xác thực sau handshake.
/// </summary>
[SerializePackable(SerializeLayout.Sequential)]
public class CredentialsPacket : IPacket, IPacketTransformer<CredentialsPacket>
{
    /// <summary>
    /// Tổng độ dài gói tin (byte), gồm header và nội dung.
    /// </summary>
    [SerializeIgnore]
    public UInt16 Length =>
        (UInt16)(PacketConstants.HeaderSize + Credentials.EstimatedSerializedLength());

    /// <summary>
    /// Magic number định danh giao thức/loại gói tin.
    /// </summary>
    [SerializeOrder(0)]
    public UInt32 MagicNumber { get; set; }

    /// <summary>
    /// Mã lệnh (opcode) của gói tin.
    /// </summary>
    [SerializeOrder(4)]
    public UInt16 OpCode { get; set; }

    /// <summary>
    /// Cờ (flags) của gói tin.
    /// </summary>
    [SerializeOrder(6)]
    public PacketFlags Flags { get; set; }

    /// <summary>
    /// Mức ưu tiên gửi/nhận gói tin.
    /// </summary>
    [SerializeOrder(7)]
    public PacketPriority Priority { get; set; }

    /// <summary>
    /// Giao thức truyền tải (TCP/UDP) của gói tin.
    /// </summary>
    [SerializeOrder(8)]
    public TransportProtocol Transport { get; set; }

    /// <summary>
    /// Thông tin đăng nhập (username, mật khẩu băm, metadata).
    /// </summary>
    [SerializeOrder(9)]
    public Credentials Credentials { get; set; }

    /// <summary>
    /// Khởi tạo mặc định với MagicNumber và Credentials rỗng.
    /// </summary>
    public CredentialsPacket()
    {
        OpCode = 0;
        MagicNumber = CommandCodes.MagicNumber;
        Credentials = new Credentials();
    }

    /// <summary>
    /// Thiết lập OpCode và Credentials.
    /// </summary>
    public void Initialize(UInt16 opCode, Credentials credentials)
    {
        OpCode = opCode;
        Credentials = credentials ?? throw new ArgumentNullException(nameof(credentials));
    }

    /// <summary>
    /// Đặt lại trạng thái để tái sử dụng từ pool.
    /// </summary>
    public void ResetForPool()
    {
        OpCode = 0;
        Credentials = new Credentials();
    }

    /// <summary>
    /// Tuần tự hoá packet thành mảng byte.
    /// </summary>
    public Byte[] Serialize() => LiteSerializer.Serialize(this);

    /// <summary>
    /// Tuần tự hoá packet vào buffer cho sẵn.
    /// </summary>
    public void Serialize(Span<Byte> buffer) => LiteSerializer.Serialize(this, buffer);

    public static CredentialsPacket Encrypt(
    CredentialsPacket packet,
    Byte[] key,
    SymmetricAlgorithmType algorithm)
    {
        if (packet?.Credentials == null)
        {
            throw new ArgumentNullException(nameof(packet));
        }

        // Convert to UTF8 bytes
        var usernameBytes = System.Text.Encoding.UTF8.GetBytes(packet.Credentials.Username ?? String.Empty);
        var passwordBytes = System.Text.Encoding.UTF8.GetBytes(packet.Credentials.Password ?? String.Empty);

        // Encrypt
        var encryptedUsername = Ciphers.Encrypt(usernameBytes, key, algorithm);
        var encryptedPassword = Ciphers.Encrypt(passwordBytes, key, algorithm);

        // Store as Base64 strings
        packet.Credentials.Username = Convert.ToBase64String(encryptedUsername.ToArray());
        packet.Credentials.Password = Convert.ToBase64String(encryptedPassword.ToArray());

        return packet;
    }

    public static CredentialsPacket Decrypt(
        CredentialsPacket packet,
        Byte[] key,
        SymmetricAlgorithmType algorithm)
    {
        if (packet?.Credentials == null)
        {
            throw new ArgumentNullException(nameof(packet));
        }

        try
        {
            // Convert from Base64 to encrypted bytes
            var encryptedUsernameBytes = Convert.FromBase64String(packet.Credentials.Username ?? String.Empty);
            var encryptedPasswordBytes = Convert.FromBase64String(packet.Credentials.Password ?? String.Empty);

            // Decrypt
            var decryptedUsernameBytes = Ciphers.Decrypt(encryptedUsernameBytes, key, algorithm);
            var decryptedPasswordBytes = Ciphers.Decrypt(encryptedPasswordBytes, key, algorithm);

            // Convert bytes back to UTF8 strings
            packet.Credentials.Username = System.Text.Encoding.UTF8.GetString(decryptedUsernameBytes.Span);
            packet.Credentials.Password = System.Text.Encoding.UTF8.GetString(decryptedPasswordBytes.Span);

            return packet;
        }
        catch (FormatException ex)
        {
            throw new InvalidOperationException("Failed to decode Base64-encoded credentials.", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to decrypt credentials.", ex);
        }
    }

    public static CredentialsPacket Compress(CredentialsPacket packet)
    {
        if (packet?.Credentials == null)
        {
            throw new ArgumentNullException(nameof(packet));
        }

        packet.Credentials.Username = packet.Credentials.Username.CompressToBase64();
        packet.Credentials.Password = packet.Credentials.Password.CompressToBase64();
        return packet;
    }

    public static CredentialsPacket Decompress(CredentialsPacket packet)
    {
        if (packet?.Credentials == null)
        {
            throw new ArgumentNullException(nameof(packet));
        }

        packet.Credentials.Username = packet.Credentials.Username.DecompressFromBase64();
        packet.Credentials.Password = packet.Credentials.Password.DecompressFromBase64();
        return packet;
    }

    public static CredentialsPacket Deserialize(ReadOnlySpan<Byte> buffer)
    {
        CredentialsPacket packet = InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>()
                                                           .Get<CredentialsPacket>();

        _ = LiteSerializer.Deserialize(buffer, ref packet);
        return packet;
    }

    public static CredentialsPacket Deserialize(in ReadOnlySpan<Byte> buffer) => throw new NotImplementedException();
}
