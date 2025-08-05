using Nalix.Common.Serialization;
using Nalix.Common.Serialization.Attributes;

namespace Nalix.NetCore.Security;

/// <summary>
/// Đại diện cho một yêu cầu thay đổi mật khẩu của người dùng.
/// Lớp này chứa thông tin về mật khẩu cũ và mật khẩu mới, được sử dụng trong quá trình xử lý thay đổi mật khẩu.
/// Được đánh dấu với thuộc tính <see cref="SerializePackableAttribute"/>
/// để hỗ trợ tuần tự hóa dữ liệu (serialization) theo bố cục tuần tự.
/// </summary>
[SerializePackable(SerializeLayout.Sequential)]
public class PasswordChange
{
    /// <summary>
    /// Mật khẩu cũ của người dùng, dùng để xác thực trước khi thay đổi.
    /// Thuộc tính này được khởi tạo mặc định là chuỗi rỗng để tránh giá trị null.
    /// Chỉ có thể được thiết lập thông qua setter private để đảm bảo tính đóng gói.
    /// </summary>
    public System.String OldPassword { get; private set; } = System.String.Empty;

    /// <summary>
    /// Mật khẩu mới mà người dùng muốn thiết lập.
    /// Thuộc tính này được khởi tạo mặc định là chuỗi rỗng để tránh giá trị null.
    /// Chỉ có thể được thiết lập thông qua setter private để đảm bảo tính đóng gói.
    /// </summary>
    public System.String NewPassword { get; private set; } = System.String.Empty;
}