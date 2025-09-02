namespace Nalix.Communication.Commands;

public enum MagicNumbers : System.UInt32
{
    Response = 0x4E414C49, // "INAL" (little-endian of "LAIN" - "NALix")
    Credentials = 0x58494C41, // "ALIX" (little-endian of "XILA" - "ALIX")
}
