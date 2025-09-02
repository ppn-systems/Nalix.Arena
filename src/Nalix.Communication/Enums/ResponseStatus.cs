namespace Nalix.Communication.Enums;

/// <summary>
/// Trạng thái phản hồi từ server cho client.
/// </summary>
public enum ResponseStatus : System.UInt16
{
    /// <summary>
    /// Thành công (Success).
    /// </summary>
    OK = 0,

    // ==== Input / Protocol errors ====

    /// <summary>
    /// Payload null hoặc sai định dạng.
    /// </summary>
    INVALID_PAYLOAD = 10,

    /// <summary>
    /// Sai thông tin đăng nhập (Invalid username/password).
    /// </summary>
    INVALID_CREDENTIALS = 11,

    /// <summary>
    /// Độ dài khóa/buffer không hợp lệ.
    /// </summary>
    INVALID_KEY_LENGTH = 12,

    /// <summary>
    /// Packet sai loại hoặc không hợp lệ cho opcode.
    /// </summary>
    INVALID_PACKET = 13,

    /// <summary>
    /// Session không hợp lệ (chưa handshake hoặc đã timeout).
    /// </summary>
    INVALID_SESSION = 14,

    // ==== Account / State errors ====

    /// <summary>
    /// Tài khoản hoặc đối tượng đã tồn tại (Conflict).
    /// </summary>
    ALREADY_EXISTS = 30,

    /// <summary>
    /// Kết nối đã handshake, không cho phép lặp lại.
    /// </summary>
    ALREADY_HANDSHAKED = 31,

    /// <summary>
    /// Tài khoản bị khóa (Locked).
    /// </summary>
    LOCKED = 32,

    /// <summary>
    /// Tài khoản bị vô hiệu hóa (Disabled).
    /// </summary>
    DISABLED = 33,

    /// <summary>
    /// Mật khẩu quá yếu (ví dụ: không đủ độ dài, không có chữ số).
    /// </summary>
    PASSWORD_TOO_WEAK = 34,

    // ==== Generic server errors ====

    /// <summary>
    /// Lỗi nội bộ server (Internal server error).
    /// </summary>
    INTERNAL_ERROR = 99
}
