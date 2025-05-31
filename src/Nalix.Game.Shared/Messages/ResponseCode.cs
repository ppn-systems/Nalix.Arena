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

    // === System/Internal Errors (1-49) ===

    /// <summary>
    /// Lỗi nội bộ không xác định, có thể do lỗi hệ thống hoặc lỗi không mong muốn trong quá trình xử lý.
    /// </summary>
    InternalError = 1,

    /// <summary>
    /// Lỗi xảy ra khi nhận được gói tin không đúng loại,
    /// ví dụ như gói tin không phải là nhị phân hoặc không phải là gói tin mong đợi.
    /// </summary>
    InvalidType = 2,

    /// <summary>
    /// Lỗi xảy ra khi độ dài của gói tin không hợp lệ,
    /// </summary>
    InvalidLength = 3,

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