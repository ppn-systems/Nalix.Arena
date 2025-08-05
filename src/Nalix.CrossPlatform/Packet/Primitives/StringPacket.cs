using Nalix.Common.Serialization;
using Nalix.Common.Serialization.Attributes;
using Nalix.NetCore.Commands;
using Nalix.NetCore.Packet.Core;

namespace Nalix.NetCore.Packet.Primitives;

[SerializePackable(SerializeLayout.Sequential)]
public class StringPacket : PacketBase
{
    public override System.UInt16 Length { get; set; }

    public override System.UInt16 OpCode { get; set; }

    public override System.UInt32 MagicNumber { get; set; }

    [SerializeDynamicSize(256)] // Hoặc size tùy theo nhu cầu của bạn
    public System.String Message { get; set; }

    public StringPacket()
    {
        Length = 0;
        OpCode = 0;
        Message = System.String.Empty;
        MagicNumber = CommandOpcodes.MagicNumber;
    }

    public void Initialize(System.UInt16 opCode, System.String message)
    {
        OpCode = opCode;
        Message = message;
        Length = (System.UInt16)(Header + System.Text.Encoding.UTF8.GetByteCount(message));
    }

    public override void ResetForPool()
    {
        Length = 0;
        OpCode = 0;
        Message = System.String.Empty;
    }
}