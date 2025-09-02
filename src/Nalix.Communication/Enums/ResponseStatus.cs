namespace Nalix.Communication.Enums;

/// <summary>
/// Trạng thái phản hồi từ server.
/// Sử dụng <see cref="System.UInt16"/> để dễ dàng mở rộng.
/// </summary>
public enum ResponseStatus : System.UInt16
{
    /// <summary>
    /// Thành công (Success).
    /// </summary>
    Ok = 0,

    /// <summary>
    /// Sai thông tin đăng nhập (Invalid username/password).
    /// </summary>
    InvalidCredentials = 1,

    /// <summary>
    /// Tài khoản hoặc đối tượng đã tồn tại (Conflict).
    /// </summary>
    AlreadyExists = 2,

    /// <summary>
    /// Tài khoản bị khóa (Locked).
    /// </summary>
    Locked = 3,

    /// <summary>
    /// Tài khoản bị vô hiệu hóa (Disabled).
    /// </summary>
    Disabled = 4,

    /// <summary>
    /// Lỗi nội bộ server (Internal server error).
    /// </summary>
    InternalError = 5
}
