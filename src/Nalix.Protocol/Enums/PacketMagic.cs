namespace Nalix.Protocol.Enums;

public enum PacketMagic : System.UInt32
{
    /// <summary>
    /// INAL
    /// </summary>
    RESPONSE = 0x4E414C49,

    /// <summary>
    /// ALIX
    /// </summary>
    CREDENTIALS = 0x58494C41,

    /// <summary>
    /// EMAG
    /// </summary>
    CHANGE_PASSWORD = 0x47414D45,
}
