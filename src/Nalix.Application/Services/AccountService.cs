using Nalix.Common.Connection;
using Nalix.Common.Packets.Attributes;
using Nalix.Common.Security.Types;
using Nalix.Communication.Commands;
using Nalix.Communication.Packet.Collections;
using Nalix.Communication.Security;
using Nalix.Cryptography.Security;
using Nalix.Infrastructure.Database;
using Nalix.Infrastructure.Repositories;
using Nalix.Logging;
using Nalix.Network.Connection;
using Nalix.Shared.Injection;
using Nalix.Shared.Memory.Pooling;
using Nalix.Shared.Messaging.Text;

namespace Nalix.Application.Services;

/// <summary>
/// Dịch vụ quản lý tài khoản người dùng với các chức năng đăng ký, đăng nhập, đăng xuất và thay đổi mật khẩu.
/// Được tối ưu hóa với Object Pooling để giảm thiểu garbage collection và cải thiện hiệu suất.
/// Tuân thủ các nguyên tắc SOLID và Domain Driven Design (DDD) cho code sạch và dễ bảo trì.
/// </summary>
[PacketController]
internal sealed class AccountService
{
    private readonly Repository<Credentials> _accounts;

    static AccountService()
    {
        _ = InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>()
                                .SetMaxCapacity<CredentialsPacket>(1024);

        _ = InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>()
                                .Prealloc<CredentialsPacket>(512);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Style", "IDE0290:Use primary constructor", Justification = "<Pending>")]
    public AccountService(GameDbContext context) => _accounts = new Repository<Credentials>(context);

    /// <summary>
    /// Đăng ký tài khoản mới cho người dùng.
    /// Sử dụng CredentialsPacket và object pooling để tối ưu hiệu suất.
    /// </summary>
    /// <param name="packet">Gói tin chứa thông tin đăng ký.</param>
    /// <param name="connection">Thông tin kết nối của client.</param>
    /// <returns>Memory chứa response packet đã được serialize.</returns>
    [PacketOpcode((System.UInt16)Command.Register)]
    [PacketPermission(PermissionLevel.Guest)]
    [PacketEncryption(true)]
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    internal async System.Threading.Tasks.Task<System.Memory<System.Byte>> RegisterAsync(
        CredentialsPacket packet, IConnection connection)
    {
        // Pattern matching theo SOLID principles
        if (packet is not CredentialsPacket credentialsPacket)
        {
            NLogix.Host.Instance.Error(
                "Invalid packet type. Expected CredentialsPacket from {0}",
                connection.RemoteEndPoint);

            return CreateResponse("Invalid packet type");
        }

        // Defensive programming
        if (credentialsPacket.Credentials is null)
        {
            NLogix.Host.Instance.Error(
                "Null credentials in register packet from {0}",
                connection.RemoteEndPoint);

            return CreateResponse("Invalid credentials");
        }

        Credentials credentials = credentialsPacket.Credentials;

        // Validation đầu vào
        if (System.String.IsNullOrWhiteSpace(credentials.Username) ||
            System.String.IsNullOrWhiteSpace(credentials.Password))
        {
            NLogix.Host.Instance.Debug(
                "Empty username or password in register attempt from {0}",
                connection.RemoteEndPoint);

            return CreateResponse("Username and password cannot be empty");
        }

        try
        {


            // Kiểm tra username đã tồn tại
            if (await _accounts.AnyAsync(a => a.Username == credentials.Username))
            {
                NLogix.Host.Instance.Warn(
                    "Username {0} already exists from connection {1}",
                    credentials.Username, connection.RemoteEndPoint);

                return CreateResponse("Username already exists");
            }

            // Tạo hash và salt cho mật khẩu
            SecureCredentials.GenerateCredentialHash(
                credentials.Password,
                out System.Byte[] salt,
                out System.Byte[] hash);

            Credentials newAccount = new()
            {
                Username = credentials.Username,
                Salt = salt,
                Hash = hash,
                Role = PermissionLevel.User,
                CreatedAt = System.DateTime.UtcNow,
                IsActive = true,
                FailedLoginCount = 0
            };

            // Thêm và lưu tài khoản mới
            _accounts.Add(newAccount);
            _ = await _accounts.SaveChangesAsync();

            // Security: Clear sensitive data
            System.Array.Clear(salt, 0, salt.Length);
            System.Array.Clear(hash, 0, hash.Length);

            NLogix.Host.Instance.Info(
                "Account {0} registered successfully from connection {1}",
                credentials.Username, connection.RemoteEndPoint);

            return CreateResponse("Registration successful");
        }
        catch (System.Exception ex)
        {
            NLogix.Host.Instance.Error(
                "Failed to register account {0} from connection {1}: {2}",
                credentials.Username, connection.RemoteEndPoint, ex.Message);

            return CreateResponse("Registration failed due to internal error");
        }
    }

    /// <summary>
    /// Xử lý yêu cầu đăng nhập của người dùng với enhanced security và object pooling.
    /// </summary>
    [PacketOpcode((System.UInt16)Command.Login)]
    [PacketPermission(PermissionLevel.Guest)]
    [PacketEncryption(true)]
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    internal async System.Threading.Tasks.Task<System.Memory<System.Byte>> LoginAsync(
        CredentialsPacket packet, IConnection connection)
    {
        if (packet is not CredentialsPacket credentialsPacket)
        {
            NLogix.Host.Instance.Error(
                "Invalid packet type. Expected CredentialsPacket from {0}",
                connection.RemoteEndPoint);

            return CreateResponse("Invalid packet type");
        }

        if (credentialsPacket.Credentials is null)
        {
            NLogix.Host.Instance.Error(
                "Null credentials in login packet from {0}",
                connection.RemoteEndPoint);

            return CreateResponse("Invalid credentials");
        }

        Credentials credentials = credentialsPacket.Credentials;

        try
        {
            // Tìm tài khoản theo username
            Credentials account = await _accounts.GetFirstOrDefaultAsync(
                a => a.Username == credentials.Username);

            if (account is null)
            {
                NLogix.Host.Instance.Warn(
                    "Login attempt with non-existent username {0} from connection {1}",
                    credentials.Username, connection.RemoteEndPoint);

                return CreateResponse("Invalid username or password");
            }

            // Kiểm tra account lockout
            if (account.FailedLoginCount >= 5 && account.LastFailedLoginAt.HasValue &&
                System.DateTime.UtcNow < account.LastFailedLoginAt.Value.AddMinutes(15))
            {
                NLogix.Host.Instance.Warn(
                    "Account {0} locked due to too many failed attempts from connection {1}",
                    credentials.Username, connection.RemoteEndPoint);

                return CreateResponse("Account temporarily locked. Try again later.");
            }

            // Xác thực mật khẩu
            if (!SecureCredentials.VerifyCredentialHash(
                credentials.Password, account.Salt, account.Hash))
            {
                // Tăng failed login count
                account.FailedLoginCount++;
                account.LastFailedLoginAt = System.DateTime.UtcNow;
                _ = await _accounts.SaveChangesAsync();

                NLogix.Host.Instance.Warn(
                    "Incorrect password for {0}, attempt {1} from connection {2}",
                    credentials.Username, account.FailedLoginCount, connection.RemoteEndPoint);

                return CreateResponse("Invalid username or password");
            }

            // Kiểm tra account active
            if (!account.IsActive)
            {
                NLogix.Host.Instance.Warn(
                    "Login attempt on disabled account {0} from connection {1}",
                    credentials.Username, connection.RemoteEndPoint);

                return CreateResponse("Account is disabled");
            }

            // Reset failed login count và update trạng thái
            account.FailedLoginCount = 0;
            account.LastFailedLoginAt = null;
            account.LastLoginAt = System.DateTime.UtcNow;
            _ = await _accounts.SaveChangesAsync();

            // Update connection state
            connection.Level = account.Role;
            InstanceManager.Instance.GetOrCreateInstance<ConnectionHub>()
                                    .AssociateUsername(connection, account.Username);

            NLogix.Host.Instance.Info(
                "User {0} logged in successfully from connection {1}",
                credentials.Username, connection.RemoteEndPoint);

            return CreateResponse("Login successful");
        }
        catch (System.Exception ex)
        {
            NLogix.Host.Instance.Error(
                "Login failed for {0} from connection {1}: {2}",
                credentials.Username, connection.RemoteEndPoint, ex.Message);

            return CreateResponse("Login failed due to internal error");
        }
    }

    /// <summary>
    /// Xử lý yêu cầu đăng xuất với proper cleanup và object pooling.
    /// </summary>
    [PacketOpcode((System.UInt16)Command.Logout)]
    [PacketPermission(PermissionLevel.User)]
    [PacketEncryption(false)]
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    internal async System.Threading.Tasks.Task<System.Memory<System.Byte>> LogoutAsync(
        CredentialsPacket ___, IConnection connection)
    {
        System.String username = InstanceManager.Instance.GetOrCreateInstance<ConnectionHub>()
                                                         .GetUsername(connection.Id);

        if (username is null)
        {
            NLogix.Host.Instance.Warn(
                "Logout attempt without valid session from connection {0}",
                connection.RemoteEndPoint);

            return CreateResponse("Invalid session");
        }

        try
        {
            Credentials account = await _accounts.GetFirstOrDefaultAsync(
                a => a.Username == username);

            if (account is not null)
            {
                account.IsActive = false;
                account.LastLogoutAt = System.DateTime.UtcNow;
                _ = await _accounts.SaveChangesAsync();
            }

            // Cleanup connection state
            connection.Level = PermissionLevel.Guest;
            _ = InstanceManager.Instance.GetOrCreateInstance<ConnectionHub>()
                                        .UnregisterConnection(connection.Id);

            NLogix.Host.Instance.Info(
                "User {0} logged out successfully from connection {1}",
                username, connection.RemoteEndPoint);

            // Disconnect after successful logout
            connection.Disconnect();

            return CreateResponse("Logout successful");
        }
        catch (System.Exception ex)
        {
            NLogix.Host.Instance.Error(
                "Logout failed for {0} from connection {1}: {2}",
                username, connection.RemoteEndPoint, ex.Message);

            // Vẫn cleanup connection state ngay cả khi có lỗi
            connection.Level = PermissionLevel.Guest;
            connection.Disconnect();

            return CreateResponse("Logout completed with warnings");
        }
    }

    /// <summary>
    /// Tạo error response packet sử dụng StringPacket với object pooling.
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static System.Memory<System.Byte> CreateResponse(System.String message)
    {
        Text256 packet = InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>()
                                                 .Get<Text256>();

        try
        {
            packet.Initialize(message);
            return new System.Memory<System.Byte>(packet.Serialize());
        }
        finally
        {
            InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>()
                                        .Return<Text256>(packet);
        }
    }
}