namespace Nalix.Game.Shared.Commands;

public enum Command : ushort
{
    /// <summary>
    /// Lệnh để người chơi đăng nhập vào trò chơi.
    /// </summary>
    Login = 0x64,

    /// <summary>
    /// Lệnh để người chơi đăng ký tài khoản mới.
    /// </summary>
    Register = 0x65,

    /// <summary>
    /// Lệnh để người chơi đăng xuất khỏi trò chơi.
    /// </summary>
    Logout = 0x66,
}