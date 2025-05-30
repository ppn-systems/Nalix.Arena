using Nalix.Common.Connection;
using Nalix.Common.Package;
using Nalix.Common.Package.Attributes;
using Nalix.Common.Security;
using Nalix.Cryptography.Security;
using Nalix.Game.Infrastructure.Database;
using Nalix.Game.Infrastructure.Repositories;
using Nalix.Game.Shared.Commands;
using Nalix.Game.Shared.Security;
using Nalix.Logging;
using Nalix.Serialization;
using System;
using System.Threading.Tasks;

namespace Nalix.Game.Application.Services;

[PacketController]
public class AccountService(GameDbContext context)
{
    private readonly Repository<Credentials> _accounts = new(context);

    /// <summary>
    /// Đăng ký tài khoản mới cho người dùng.
    /// Phương thức này xử lý yêu cầu đăng ký bằng cách kiểm tra xem tên người dùng đã tồn tại chưa,
    /// tạo hash mật khẩu an toàn và lưu tài khoản mới vào cơ sở dữ liệu.
    /// </summary>
    /// <param name="packet">Gói tin chứa thông tin đăng ký (tên người dùng, mật khẩu).</param>
    /// <param name="connection">Thông tin kết nối của client.</param>
    /// <returns>Chuỗi thông báo kết quả của quá trình đăng ký.</returns>
    [PacketOpcode((ushort)Command.Register)]
    [PacketPermission(PermissionLevel.Guest)]
    internal async Task<string> RegisterAsync(IPacket packet, IConnection connection)
    {
        Credentials credentials = new();
        BitSerializer.Deserialize(packet.Payload.Span, ref credentials);

        // Kiểm tra xem tên người dùng đã tồn tại trong cơ sở dữ liệu chưa
        if (await _accounts.AnyAsync(a => a.Username == credentials.Username))
        {
            NLogix.Host.Instance.Warn(
                $"Username {0} already exists from connection {1}",
                credentials.Username, connection.RemoteEndPoint);

            return "Username already existed.";
        }

        try
        {
            // Tạo hash và salt cho mật khẩu để lưu trữ an toàn
            SecureCredentials.GenerateCredentialHash(credentials.Password, out byte[] salt, out byte[] hash);
            Credentials newAccount = new()
            {
                Username = credentials.Username,
                Salt = salt,
                Hash = hash,
                Role = PermissionLevel.User,
                CreatedAt = DateTime.UtcNow
            };

            // Thêm tài khoản mới vào cơ sở dữ liệu và lưu thay đổi
            _accounts.Add(newAccount);
            await _accounts.SaveChangesAsync();
            NLogix.Host.Instance.Info(
                "Account {0} registered successfully from connection {1}",
                credentials.Username, connection.RemoteEndPoint);

            return "Account registered successfully.";
        }
        catch (Exception ex)
        {
            // Ghi log lỗi nếu quá trình đăng ký thất bại
            NLogix.Host.Instance.Error(
                "Failed to register account {0} from connection {1}, Ex: {2}",
                credentials.Username, connection.RemoteEndPoint, ex);

            return "Failed to register account due to an internal error.";
        }
    }

    /// <summary>
    /// Xử lý yêu cầu đăng nhập của người dùng.
    /// Phương thức này xác thực thông tin đăng nhập, kiểm tra trạng thái tài khoản,
    /// và cập nhật trạng thái đăng nhập nếu thành công.
    /// </summary>
    /// <param name="packet">Gói tin chứa thông tin đăng nhập (tên người dùng, mật khẩu).</param>
    /// <param name="connection">Thông tin kết nối của client.</param>
    /// <returns>Chuỗi thông báo kết quả của quá trình đăng nhập.</returns>
    [PacketOpcode((ushort)Command.Login)]
    [PacketPermission(PermissionLevel.Guest)]
    public async Task<string> LoginAsync(IPacket packet, IConnection connection)
    {
        Credentials credentials = new();
        BitSerializer.Deserialize(packet.Payload.Span, ref credentials);

        // Tìm tài khoản trong cơ sở dữ liệu theo tên người dùng
        Credentials account = await _accounts.GetFirstOrDefaultAsync(a => a.Username == credentials.Username);
        if (account == null)
        {
            NLogix.Host.Instance.Warn(
                "Login account attempt with non-existent username {0} from connection {1}",
                credentials.Username, connection.RemoteEndPoint);

            return "Username does not exist.";
        }

        // Kiểm tra nếu tài khoản bị khóa do quá nhiều lần đăng nhập thất bại
        if (account.FailedLoginCount >= 5 && account.LastFailedLoginAt.HasValue &&
            DateTime.UtcNow < account.LastFailedLoginAt.Value.AddMinutes(15))
        {
            NLogix.Host.Instance.Warn(
                "Account {0} locked due to too many failed attempts from connection {1}",
                credentials.Username, connection.RemoteEndPoint);

            return "Account locked due to too many failed attempts.";
        }

        // Xác thực mật khẩu
        if (!SecureCredentials.VerifyCredentialHash(credentials.Password, account.Salt, account.Hash))
        {
            // Tăng số lần đăng nhập thất bại và cập nhật thời gian thất bại
            account.FailedLoginCount++;
            account.LastFailedLoginAt = DateTime.UtcNow;

            await _accounts.SaveChangesAsync();
            NLogix.Host.Instance.Warn(
                "Incorrect password for {0}, attempt {1} from connection {2}",
                credentials.Username, account.FailedLoginCount, connection.RemoteEndPoint);

            return "Incorrect password.";
        }

        // Kiểm tra trạng thái hoạt động của tài khoản
        if (!account.IsActive)
        {
            NLogix.Host.Instance.Warn(
                "Login account attempt on disabled account {0} from connection {1}",
                credentials.Username, connection.RemoteEndPoint);

            return "Account is disabled.";
        }

        try
        {
            // Đặt lại số lần đăng nhập thất bại và cập nhật trạng thái tài khoản
            account.FailedLoginCount = 0;
            account.IsActive = true;
            await _accounts.SaveChangesAsync();

            // Cập nhật thông tin kết nối với quyền và tên người dùng
            connection.Level = account.Role;
            connection.Metadata["Username"] = account.Username;
            NLogix.Host.Instance.Info(
                $"User {0} logged in successfully from connection {1}",
                credentials.Username, connection.RemoteEndPoint);

            return "Login successful.";
        }
        catch (Exception ex)
        {
            // Ghi log lỗi nếu quá trình đăng nhập thất bại
            NLogix.Host.Instance.Error(
                "Failed to complete login for {0} from connection {1}, Ex: {2}",
                credentials.Username, connection.RemoteEndPoint, ex);

            return "Failed to login due to an internal error.";
        }
    }

    /// <summary>
    /// Xử lý yêu cầu đăng xuất của người dùng.
    /// Phương thức này cập nhật trạng thái tài khoản, xóa thông tin phiên và ngắt kết nối.
    /// </summary>
    /// <param name="_">Gói tin (không sử dụng).</param>
    /// <param name="connection">Thông tin kết nối của client.</param>
    /// <returns>Chuỗi thông báo kết quả của quá trình đăng xuất.</returns>
    [PacketOpcode((ushort)Command.Logout)]
    [PacketPermission(PermissionLevel.User)]
    internal async Task<string> LogoutAsync(IPacket _, IConnection connection)
    {
        // Kiểm tra xem phiên có chứa tên người dùng hợp lệ không
        if (!connection.Metadata.TryGetValue("Username", out object value) || value is not string username)
        {
            NLogix.Host.Instance.Warn(
                "Logout attempt without valid username metadata from connection {0}",
                connection.RemoteEndPoint);

            return "Invalid session. Please login again.";
        }

        // Tìm tài khoản trong cơ sở dữ liệu
        Credentials account = await _accounts.GetFirstOrDefaultAsync(a => a.Username == username);
        if (account == null)
        {
            NLogix.Host.Instance.Warn(
                "Logout attempt with non-existent username {0} from connection {1}",
                username, connection.RemoteEndPoint);

            return "Username does not exist.";
        }

        try
        {
            // Cập nhật trạng thái tài khoản thành không hoạt động
            account.IsActive = false;
            await _accounts.SaveChangesAsync();
        }
        catch (Exception)
        {
            // Ghi log lỗi nhưng vẫn tiếp tục đăng xuất
            NLogix.Host.Instance.Info("User {0} logged out from connection {1}", username, connection.RemoteEndPoint);
            return "Failed to update account status.";
        }

        // Ghi log sự kiện đăng xuất
        NLogix.Host.Instance.Info(
            "User {0} logged out from connection {1}",
            username, connection.RemoteEndPoint);

        // Xóa thông tin metadata và hạ quyền về mức Guest
        connection.Level = PermissionLevel.Guest;
        connection.Metadata.Remove("Username");

        // Ngắt kết nối
        connection.Disconnect();

        return "Logout successful.";
    }

    /// <summary>
    /// Xử lý yêu cầu thay đổi mật khẩu của người dùng.
    /// Phương thức này xác thực mật khẩu cũ, tạo hash mật khẩu mới và cập nhật vào cơ sở dữ liệu.
    /// </summary>
    /// <param name="packet">Gói tin chứa thông tin mật khẩu cũ và mới.</param>
    /// <param name="connection">Thông tin kết nối của client.</param>
    /// <returns>Chuỗi thông báo kết quả của quá trình thay đổi mật khẩu.</returns>
    [PacketOpcode((ushort)Command.ChangePassword)]
    [PacketPermission(PermissionLevel.User)]
    internal async Task<string> ChangePasswordAsync(IPacket packet, IConnection connection)
    {
        // Kiểm tra xem phiên có chứa tên người dùng hợp lệ không
        if (!connection.Metadata.TryGetValue("Username", out object value) || value is not string username)
        {
            NLogix.Host.Instance.Warn(
                "Change password attempt without valid username metadata from connection {0}",
                connection.RemoteEndPoint);

            return "Invalid session. Please login again.";
        }

        // Phân tích yêu cầu thay đổi mật khẩu
        if (!PasswordChange.TryParse(packet.Payload.Span, out var request))
            return "Invalid password change format.";

        // Tìm tài khoản trong cơ sở dữ liệu
        Credentials account = await _accounts.GetFirstOrDefaultAsync(a => a.Username == username);
        if (account == null)
            return "Account does not exist.";

        // Xác thực mật khẩu cũ
        if (!SecureCredentials.VerifyCredentialHash(request.OldPassword, account.Salt, account.Hash))
            return "Old password is incorrect.";

        try
        {
            // Tạo hash và salt mới cho mật khẩu
            SecureCredentials.GenerateCredentialHash(request.NewPassword, out byte[] salt, out byte[] hash);
            account.Salt = salt;
            account.Hash = hash;
            await _accounts.SaveChangesAsync();

            // Ghi log sự kiện thay đổi mật khẩu
            NLogix.Host.Instance.Info(
                "User {0} changed password successfully from connection {1}",
                username, connection.RemoteEndPoint);

            return "Password changed successfully.";
        }
        catch (Exception ex)
        {
            // Ghi log lỗi nếu quá trình thay đổi mật khẩu thất bại
            NLogix.Host.Instance.Error(
                "Failed to change password for {0} from connection {1}, Ex: {2}",
                username, connection.RemoteEndPoint, ex);

            return "Failed to change password due to internal error.";
        }
    }
}