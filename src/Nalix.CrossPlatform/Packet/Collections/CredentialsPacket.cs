using Nalix.Common.Serialization;
using Nalix.Common.Serialization.Attributes;
using Nalix.NetCore.Commands;
using Nalix.NetCore.Packet.Core;
using Nalix.NetCore.Security;

namespace Nalix.NetCore.Packet.Collections;

/// <summary>
/// Packet chứa thông tin đăng nhập từ client (username, password hash, v.v.).
/// Được sử dụng trong quá trình xác thực sau khi handshake.
/// </summary>
[SerializePackable(SerializeLayout.Sequential)]
public class CredentialsPacket : PacketBase
{
    public override System.UInt16 Length { get; set; }

    public override System.UInt16 OpCode { get; set; }

    public override System.UInt32 MagicNumber { get; set; }

    /// <summary>
    /// Thông tin đăng nhập bao gồm username, hashed password, và các metadata.
    /// </summary>
    public Credentials Credentials { get; set; }

    public CredentialsPacket()
    {
        Length = 0;
        OpCode = 0;
        MagicNumber = CommandOpcodes.MagicNumber;
        Credentials = new Credentials(); // đảm bảo không null để tránh lỗi serialize
    }

    /// <summary>
    /// Khởi tạo packet với opcode và credentials.
    /// </summary>
    public void Initialize(System.UInt16 opCode, Credentials credentials)
    {
        Credentials = credentials ?? throw new System.ArgumentNullException(nameof(credentials));

        OpCode = opCode;
        // Ước lượng độ dài dựa trên nội dung credentials đã được serialize
        Length = (System.UInt16)(Header + credentials.EstimatedSerializedLength());
    }

    public override void ResetForPool()
    {
        Length = 0;
        OpCode = 0;
        Credentials = new Credentials();
    }
}
