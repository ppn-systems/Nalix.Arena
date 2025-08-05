namespace Nalix.NetCore.Commands;

/// <summary>
/// Chứa các hằng số opcode tương ứng với enum Command.
/// Dùng để gán trong các attribute yêu cầu constant expression.
/// </summary>
internal static class CommandOpcodes
{
    public const System.UInt32 MagicNumber/**/= 0x4E414C58;

    public const System.UInt16 Handshake /**/ = (System.UInt16)Command.Handshake;
    public const System.UInt16 Login /*    */ = (System.UInt16)Command.Login;
    public const System.UInt16 Logout /*   */ = (System.UInt16)Command.Logout;
    public const System.UInt16 Register /* */ = (System.UInt16)Command.Register;
    public const System.UInt16 ChangePassword = (System.UInt16)Command.ChangePassword;
}