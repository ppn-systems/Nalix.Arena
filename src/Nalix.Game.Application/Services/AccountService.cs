using Nalix.Common.Attributes;
using Nalix.Common.Connection;
using Nalix.Common.Package;
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

    [PacketOpcode((ushort)Command.Register)]
    [PacketPermission(PermissionLevel.Guest)]
    internal async Task<string> RegisterAsync(IPacket packet, IConnection connection)
    {
        Credentials credentials = new();
        BitSerializer.Deserialize(packet.Payload.Span, ref credentials);

        if (await _accounts.AnyAsync(a => a.Username == credentials.Username))
        {
            NLogix.Host.Instance.Warn(
                $"Username {0} already exists from connection {1}",
                credentials.Username, connection.RemoteEndPoint);

            return "Username already existed.";
        }

        try
        {
            SecureCredentials.GenerateCredentialHash(credentials.Password, out byte[] salt, out byte[] hash);
            Credentials newAccount = new()
            {
                Username = credentials.Username,
                Salt = salt,
                Hash = hash,
                Role = PermissionLevel.User,
                CreatedAt = DateTime.UtcNow
            };

            _accounts.Add(newAccount);
            await _accounts.SaveChangesAsync();
            NLogix.Host.Instance.Info(
                "Account {0} registered successfully from connection {1}",
                credentials.Username, connection.RemoteEndPoint);

            return "Account registered successfully.";
        }
        catch (Exception ex)
        {
            NLogix.Host.Instance.Error(
                "Failed to register account {0} from connection {1}, Ex: {2}",
                credentials.Username, connection.RemoteEndPoint, ex);

            return "Failed to register account due to an internal error.";
        }
    }

    [PacketOpcode((ushort)Command.Login)]
    [PacketPermission(PermissionLevel.Guest)]
    public async Task<string> LoginAsync(IPacket packet, IConnection connection)
    {
        Credentials credentials = new();
        BitSerializer.Deserialize(packet.Payload.Span, ref credentials);

        Credentials account = await _accounts.GetFirstOrDefaultAsync(a => a.Username == credentials.Username);
        if (account == null)
        {
            NLogix.Host.Instance.Warn(
                "Login account attempt with non-existent username {0} from connection {1}",
                credentials.Username, connection.RemoteEndPoint);

            return "Username does not exist.";
        }

        if (account.FailedLoginCount >= 5 && account.LastFailedLoginAt.HasValue &&
            DateTime.UtcNow < account.LastFailedLoginAt.Value.AddMinutes(15))
        {
            NLogix.Host.Instance.Warn(
                "Account {0} locked due to too many failed attempts from connection {1}",
                credentials.Username, connection.RemoteEndPoint);

            return "Account locked due to too many failed attempts.";
        }

        if (!SecureCredentials.VerifyCredentialHash(credentials.Password, account.Salt, account.Hash))
        {
            account.FailedLoginCount++;
            account.LastFailedLoginAt = DateTime.UtcNow;

            await _accounts.SaveChangesAsync();
            NLogix.Host.Instance.Warn(
                "Incorrect password for {0}, attempt {1} from connection {2}",
                credentials.Username, account.FailedLoginCount, connection.RemoteEndPoint);

            return "Incorrect password.";
        }

        if (!account.IsActive)
        {
            NLogix.Host.Instance.Warn(
                "Login account attempt on disabled account {0} from connection {1}",
                credentials.Username, connection.RemoteEndPoint);

            return "Account is disabled.";
        }

        try
        {
            account.FailedLoginCount = 0;
            account.IsActive = true;
            await _accounts.SaveChangesAsync();

            connection.Level = account.Role;
            connection.Metadata["Username"] = account.Username;
            NLogix.Host.Instance.Info(
                $"User {0} logged in successfully from connection {1}",
                credentials.Username, connection.RemoteEndPoint);

            return "Login successful.";
        }
        catch (Exception ex)
        {
            NLogix.Host.Instance.Error(
                "Failed to complete login for {0} from connection {1}, Ex: {2}",
                credentials.Username, connection.RemoteEndPoint, ex);

            return "Failed to login due to an internal error.";
        }
    }

    [PacketOpcode((ushort)Command.Logout)]
    [PacketPermission(PermissionLevel.User)]
    internal async Task<string> LogoutAsync(IPacket _, IConnection connection)
    {
        if (!connection.Metadata.TryGetValue("Username", out object value) || value is not string username)
        {
            NLogix.Host.Instance.Warn(
                "Logout attempt without valid username metadata from connection {0}",
                connection.RemoteEndPoint);

            return "Invalid session. Please login again.";
        }

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
            account.IsActive = false;
            await _accounts.SaveChangesAsync();
        }
        catch (Exception)
        {
            // Log the error
            NLogix.Host.Instance.Info("User {0} logged out from connection {1}", username, connection.RemoteEndPoint);
            return "Failed to update account status.";
        }

        // Ghi log sự kiện đăng xuất
        NLogix.Host.Instance.Info(
            "User {0} logged out from connection {1}",
            username, connection.RemoteEndPoint);

        // Xóa thông tin metadata và hạ quyền
        connection.Level = PermissionLevel.Guest;
        connection.Metadata.Remove("Username");

        // Ngắt kết nối nếu cần
        connection.Disconnect();

        return "Logout successful.";
    }

    [PacketOpcode((ushort)Command.ChangePassword)]
    [PacketPermission(PermissionLevel.User)]
    internal async Task<string> ChangePasswordAsync(IPacket packet, IConnection connection)
    {
        if (!connection.Metadata.TryGetValue("Username", out object value) || value is not string username)
        {
            NLogix.Host.Instance.Warn(
                "Change password attempt without valid username metadata from connection {0}",
                connection.RemoteEndPoint);

            return "Invalid session. Please login again.";
        }

        if (!PasswordChange.TryParse(packet.Payload.Span, out var request))
            return "Invalid password change format.";

        Credentials account = await _accounts.GetFirstOrDefaultAsync(a => a.Username == username);
        if (account == null)
            return "Account does not exist.";

        if (!SecureCredentials.VerifyCredentialHash(request.OldPassword, account.Salt, account.Hash))
            return "Old password is incorrect.";

        try
        {
            SecureCredentials.GenerateCredentialHash(request.NewPassword, out byte[] salt, out byte[] hash);
            account.Salt = salt;
            account.Hash = hash;
            await _accounts.SaveChangesAsync();

            NLogix.Host.Instance.Info(
                "User {0} changed password successfully from connection {1}",
                username, connection.RemoteEndPoint);

            return "Password changed successfully.";
        }
        catch (Exception ex)
        {
            NLogix.Host.Instance.Error(
                "Failed to change password for {0} from connection {1}, Ex: {2}",
                username, connection.RemoteEndPoint, ex);

            return "Failed to change password due to internal error.";
        }
    }
}