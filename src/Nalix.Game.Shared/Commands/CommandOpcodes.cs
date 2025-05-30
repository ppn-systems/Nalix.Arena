namespace Nalix.Game.Shared.Commands;

/// <summary>
/// Chứa các hằng số opcode tương ứng với enum Command.
/// Dùng để gán trong các attribute yêu cầu constant expression.
/// </summary>
internal static class CommandOpcodes
{
    public const ushort Handshake /**/ = (ushort)Command.Handshake;
    public const ushort Login /*    */ = (ushort)Command.Login;
    public const ushort Logout /*   */ = (ushort)Command.Logout;
    public const ushort Register /* */ = (ushort)Command.Register;
    public const ushort ChangePassword = (ushort)Command.ChangePassword;
}