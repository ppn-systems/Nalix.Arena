namespace Nalix.Game.Shared.Messages;

/// <summary>
/// Định nghĩa các mã phản hồi (response codes) dùng để biểu thị trạng thái hoặc lỗi trong quá trình xử lý yêu cầu.
/// Các mã này được sử dụng trong các giao tiếp giữa client và server để thông báo kết quả thực hiện các thao tác.
/// </summary>
public enum ResponseCode : System.UInt16
{
    /// <summary>
    /// Biểu thị thao tác được thực hiện thành công, không có lỗi xảy ra.
    /// </summary>
    Success = 0,

    /// <summary>
    /// Lỗi nội bộ xảy ra trong quá trình xử lý yêu cầu, thường do vấn đề hệ thống hoặc cơ sở dữ liệu.
    /// </summary>
    InternalError = 1,

    /// <summary>
    /// Mật khẩu cung cấp không hợp lệ, không đúng định dạng hoặc không đáp ứng các yêu cầu bảo mật.
    /// </summary>
    InvalidPassword = 100,

    /// <summary>
    /// Người dùng không được tìm thấy trong hệ thống, có thể do tên người dùng không tồn tại.
    /// </summary>
    UserNotFound = 101,

    /// <summary>
    /// Yêu cầu thiếu tham số bắt buộc để xử lý, ví dụ như thiếu tên người dùng hoặc mật khẩu.
    /// </summary>
    MissingParameter = 102,
}