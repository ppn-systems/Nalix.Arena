using Nalix.Common.Attributes;
using Nalix.Common.Caching;
using Nalix.Common.Enums;
using Nalix.Common.Packets;
using Nalix.Common.Packets.Abstractions;
using Nalix.Common.Packets.Enums;
using Nalix.Common.Serialization.Attributes;
using Nalix.Common.Serialization.Enums;
using Nalix.Communication.Enums;
using Nalix.Communication.Extensions;
using Nalix.Communication.Models;
using Nalix.Cryptography.Extensions;
using Nalix.Framework.Injection;
using Nalix.Shared.LZ4.Extensions;
using Nalix.Shared.Memory.Pooling;
using Nalix.Shared.Messaging;
using Nalix.Shared.Serialization;
using System;

namespace Nalix.Communication.Collections;

/// <summary>
/// Gói tin chứa thông tin đăng nhập từ client (username, mật khẩu băm, metadata),
/// dùng trong quá trình xác thực sau handshake.
/// </summary>
[SerializePackable(SerializeLayout.Explicit)]
[MagicNumber((UInt32)PacketMagic.CREDENTIALS)]
public class CredentialsPacket : FrameBase, IPoolable, IPacketTransformer<CredentialsPacket>
{
    /// <summary>
    /// Tổng độ dài gói tin (byte), gồm header và nội dung.
    /// </summary>
    [SerializeIgnore]
    public override UInt16 Length =>
        (UInt16)(PacketConstants.HeaderSize + Credentials.EstimatedSerializedLength());

    /// <summary>
    /// Thông tin đăng nhập (username, mật khẩu băm, metadata).
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.DataRegion)]
    public Credentials Credentials { get; set; }

    /// <summary>
    /// Khởi tạo mặc định với MagicNumber và CREDENTIALS rỗng.
    /// </summary>
    public CredentialsPacket()
    {
        OpCode = OpCommand.NONE.AsUInt16();
        MagicNumber = PacketMagic.CREDENTIALS.AsUInt32();
        Credentials = new Credentials();
    }

    /// <summary>
    /// Thiết lập OpCode và CREDENTIALS.
    /// </summary>
    public void Initialize(UInt16 opCode, Credentials credentials)
    {
        OpCode = opCode;
        Credentials = credentials ?? throw new ArgumentNullException(nameof(credentials));
    }

    /// <summary>
    /// Đặt lại trạng thái để tái sử dụng từ pool.
    /// </summary>
    public override void ResetForPool()
    {
        OpCode = OpCommand.NONE.AsUInt16();
        Credentials = new Credentials();
    }

    public static CredentialsPacket Encrypt(
    CredentialsPacket packet,
    Byte[] key,
    SymmetricAlgorithmType algorithm)
    {
        if (packet?.Credentials == null)
        {
            throw new ArgumentNullException(nameof(packet));
        }

        packet.Credentials.Username = packet.Credentials.Username.EncryptToBase64(key, algorithm);
        packet.Credentials.Password = packet.Credentials.Password.EncryptToBase64(key, algorithm);

        packet.Flags |= PacketFlags.Encrypted;

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
            packet.Credentials.Username = packet.Credentials.Username.DecryptFromBase64(key, algorithm);
            packet.Credentials.Password = packet.Credentials.Password.DecryptFromBase64(key, algorithm);

            packet.Flags &= ~PacketFlags.Encrypted;

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

        packet.Flags |= PacketFlags.Compressed;

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

        packet.Flags &= ~PacketFlags.Compressed;

        return packet;
    }

    public static CredentialsPacket Deserialize(ReadOnlySpan<Byte> buffer)
    {
        CredentialsPacket packet = InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>()
                                                           .Get<CredentialsPacket>();

        _ = LiteSerializer.Deserialize(buffer, ref packet);
        return packet;
    }

    /// <inheritdoc/>
    public override System.Byte[] Serialize() => LiteSerializer.Serialize(this);

    /// <inheritdoc/>
    public override Int32 Serialize(System.Span<System.Byte> buffer) => LiteSerializer.Serialize(this, buffer);
}
