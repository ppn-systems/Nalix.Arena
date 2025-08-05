using Nalix.Common.Connection.Protocols;
using Nalix.Common.Packets;
using Nalix.Common.Packets.Enums;
using Nalix.Common.Security.Cryptography;
using Nalix.Common.Serialization;
using Nalix.Common.Serialization.Attributes;
using Nalix.Shared.Serialization;

namespace Nalix.NetCore.Packet.Core;

[SerializePackable(SerializeLayout.Sequential)]
public abstract class PacketBase : IPacket, IPacketTransformer<PacketBase>
{
    public const System.UInt16 Header = sizeof(PacketFlags) + sizeof(PacketPriority) +
        sizeof(TransportProtocol) + sizeof(System.UInt16) + sizeof(System.UInt16) + sizeof(System.UInt32);

    // Default implementations (virtual để override khi cần)
    public virtual PacketFlags Flags => PacketFlags.None;
    public virtual PacketPriority Priority => PacketPriority.Low;
    public virtual TransportProtocol Transport => TransportProtocol.Tcp;

    [SerializeIgnore]
    public virtual System.Int32 Hash => 0;

    // Require subclasses to implement
    public abstract System.UInt16 Length { get; set; }
    public abstract System.UInt16 OpCode { get; set; }
    public abstract System.UInt32 MagicNumber { get; set; }

    // Serialization logic (you may move it to a helper if needed)
    public virtual System.Byte[] Serialize() => LiteSerializer.Serialize(this);

    public virtual void Serialize(System.Span<System.Byte> buffer) => LiteSerializer.Serialize(this, buffer);

    public virtual void ResetForPool() { }

    // Implement IPacketTransformer<PacketBase> methods with default or throw
    public virtual PacketBase Create(System.UInt16 id, System.String s) => throw new System.NotImplementedException();
    public virtual PacketBase Create(System.UInt16 id, PacketFlags flags) => throw new System.NotImplementedException();
    public virtual PacketBase Encrypt(PacketBase packet, System.Byte[] key, SymmetricAlgorithmType algorithm)
        => throw new System.NotImplementedException();
    public virtual PacketBase Decrypt(PacketBase packet, System.Byte[] key, SymmetricAlgorithmType algorithm)
        => throw new System.NotImplementedException();
    public virtual PacketBase Compress(PacketBase packet) => throw new System.NotImplementedException();
    public virtual PacketBase Decompress(PacketBase packet) => throw new System.NotImplementedException();
    public virtual PacketBase Deserialize(System.ReadOnlySpan<System.Byte> buffer) => throw new System.NotImplementedException();
}
