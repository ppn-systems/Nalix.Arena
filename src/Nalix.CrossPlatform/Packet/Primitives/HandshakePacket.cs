using Nalix.Common.Serialization;
using Nalix.Common.Serialization.Attributes;
using Nalix.NetCore.Commands;
using Nalix.NetCore.Packet.Core;

namespace Nalix.NetCore.Packet.Primitives;

[SerializePackable(SerializeLayout.Sequential)]
public class HandshakePacket : PacketBase
{
    public override System.UInt16 Length { get; set; }

    public override System.UInt16 OpCode { get; set; }

    public override System.UInt32 MagicNumber { get; set; }

    [SerializeDynamicSize(32)]
    public System.Byte[] Payload { get; set; }

    public HandshakePacket()
    {
        Length = 0;
        OpCode = 0;
        Payload = [];
        MagicNumber = CommandOpcodes.MagicNumber;
    }

    public void Initialize(System.UInt16 opCode, System.Byte[] payload)
    {
        if (payload.Length != 32)
        {
            throw new System.ArgumentException("Payload must be exactly 32 bytes long for X25519 public key.");
        }

        OpCode = opCode;
        Payload = payload;
        Length = Header + 32;
    }

    public override void ResetForPool()
    {
        Length = 0;
        OpCode = 0;
        Payload = [];
    }
}
