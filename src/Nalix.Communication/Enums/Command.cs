namespace Nalix.Communication.Enums;

public enum Command : System.UInt16
{
    /// <summary>
    /// Lệnh không xác định hoặc không có chức năng cụ thể.
    /// </summary>
    NONE = 0x00,

    /// <summary>
    /// Lệnh để thiết lập kết nối ban đầu giữa máy khách và máy chủ.
    /// </summary>
    HANDSHAKE = 0x01,

    /// <summary>
    /// Lệnh để người chơi đăng nhập vào trò chơi.
    /// </summary>
    LOGIN = 0x64,

    /// <summary>
    /// Lệnh để người chơi đăng xuất khỏi trò chơi.
    /// </summary>
    LOGOUT = 0x65,

    /// <summary>
    /// Lệnh để người chơi đăng ký tài khoản mới.
    /// </summary>
    REGISTER = 0x66,

    /// <summary>
    /// Lệnh để người chơi thay đổi mật khẩu tài khoản.
    /// </summary>
    CHANGE_PASSWORD = 0x67,
}