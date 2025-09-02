using Nalix.Application.Extensions;
using Nalix.Common.Connection;
using Nalix.Common.Packets.Attributes;
using Nalix.Common.Security.Types;
using Nalix.Communication.Collections;
using Nalix.Communication.Enums;
using Nalix.Communication.Security;
using Nalix.Cryptography.Security;
using Nalix.Infrastructure.Database;
using Nalix.Infrastructure.Repositories;
using Nalix.Logging;
using Nalix.Network.Connection;
using Nalix.Shared.Injection;
using Nalix.Shared.Memory.Pooling;
using System;
using System.Threading.Tasks;

namespace Nalix.Application.Operations.Security;

/// <summary>
/// User account management service: register, login, logout with secure practices and pooling.
/// </summary>
[PacketController]
internal sealed class AccountOps
{
    private readonly Repository<Credentials> _accounts;

    static AccountOps()
    {
        _ = InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>()
                                    .SetMaxCapacity<CredentialsPacket>(1024);

        _ = InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>()
                                    .Prealloc<CredentialsPacket>(512);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Style", "IDE0290:Use primary constructor", Justification = "<Pending>")]
    public AccountOps(GameDbContext context) => _accounts = new Repository<Credentials>(context);

    /// <summary>
    /// Handles user registration.
    /// </summary>
    [PacketOpcode((UInt16)Command.REGISTER)]
    [PacketPermission(PermissionLevel.Guest)]
    [PacketEncryption(true)]
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    internal async Task RegisterAsync(CredentialsPacket packet, IConnection connection)
    {
        const UInt16 Op = (UInt16)Command.REGISTER;

        // Validate packet type (defensive even though signature is CredentialsPacket)
        if (packet is not CredentialsPacket credentialsPacket)
        {
            NLogix.Host.Instance.Error(
                "Invalid packet type. Expected CredentialsPacket from {0}",
                connection.RemoteEndPoint);

            await connection.SendAsync(Op, ResponseStatus.INVALID_PACKET).ConfigureAwait(false);
            return;
        }

        // Null payload
        if (credentialsPacket.Credentials is null)
        {
            NLogix.Host.Instance.Error(
                "Null credentials in register packet from {0}",
                connection.RemoteEndPoint);

            await connection.SendAsync(Op, ResponseStatus.INVALID_PAYLOAD).ConfigureAwait(false);
            return;
        }

        Credentials credentials = credentialsPacket.Credentials;

        // Basic input validation
        if (String.IsNullOrWhiteSpace(credentials.Username) ||
            String.IsNullOrWhiteSpace(credentials.Password))
        {
            NLogix.Host.Instance.Debug(
                "Empty username or password in register attempt from {0}",
                connection.RemoteEndPoint);

            await connection.SendAsync(Op, ResponseStatus.INVALID_PAYLOAD).ConfigureAwait(false);
            return;
        }

        try
        {
            // Check existing username
            if (await _accounts.AnyAsync(a => a.Username == credentials.Username).ConfigureAwait(false))
            {
                NLogix.Host.Instance.Warn(
                    "Username {0} already exists from connection {1}",
                    credentials.Username, connection.RemoteEndPoint);

                await connection.SendAsync(Op, ResponseStatus.ALREADY_EXISTS).ConfigureAwait(false);
                return;
            }

            // Derive salt/hash
            SecureCredentials.GenerateCredentialHash(
                credentials.Password,
                out Byte[] salt,
                out Byte[] hash);

            Credentials newAccount = new()
            {
                Username = credentials.Username,
                Salt = salt,
                Hash = hash,
                Role = PermissionLevel.User,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                FailedLoginCount = 0
            };

            _accounts.Add(newAccount);
            _ = await _accounts.SaveChangesAsync().ConfigureAwait(false);

            // Clear sensitive
            Array.Clear(salt, 0, salt.Length);
            Array.Clear(hash, 0, hash.Length);

            NLogix.Host.Instance.Info(
                "Account {0} registered successfully from connection {1}",
                credentials.Username, connection.RemoteEndPoint);

            await connection.SendAsync(Op, ResponseStatus.OK).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            NLogix.Host.Instance.Error(
                "Failed to register account {0} from connection {1}: {2}",
                credentials.Username, connection.RemoteEndPoint, ex.Message);

            await connection.SendAsync(Op, ResponseStatus.INTERNAL_ERROR).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Handles user login.
    /// </summary>
    [PacketOpcode((UInt16)Command.LOGIN)]
    [PacketPermission(PermissionLevel.Guest)]
    [PacketEncryption(true)]
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    internal async Task LoginAsync(CredentialsPacket packet, IConnection connection)
    {
        const UInt16 Op = (UInt16)Command.LOGIN;

        if (packet is not CredentialsPacket credentialsPacket)
        {
            NLogix.Host.Instance.Error(
                "Invalid packet type. Expected CredentialsPacket from {0}",
                connection.RemoteEndPoint);

            await connection.SendAsync(Op, ResponseStatus.INVALID_PACKET).ConfigureAwait(false);
            return;
        }

        if (credentialsPacket.Credentials is null)
        {
            NLogix.Host.Instance.Error(
                "Null credentials in login packet from {0}",
                connection.RemoteEndPoint);

            await connection.SendAsync(Op, ResponseStatus.INVALID_PAYLOAD).ConfigureAwait(false);
            return;
        }

        Credentials credentials = credentialsPacket.Credentials;

        try
        {
            // Look up account
            Credentials account = await _accounts.GetFirstOrDefaultAsync(
                a => a.Username == credentials.Username).ConfigureAwait(false);

            if (account is null)
            {
                NLogix.Host.Instance.Warn(
                    "LOGIN attempt with non-existent username {0} from connection {1}",
                    credentials.Username, connection.RemoteEndPoint);

                await connection.SendAsync(Op, ResponseStatus.INVALID_CREDENTIALS).ConfigureAwait(false);
                return;
            }

            // Lockout window
            if (account.FailedLoginCount >= 5 &&
                account.LastFailedLoginAt.HasValue &&
                DateTime.UtcNow < account.LastFailedLoginAt.Value.AddMinutes(15))
            {
                NLogix.Host.Instance.Warn(
                    "Account {0} locked due to too many failed attempts from connection {1}",
                    credentials.Username, connection.RemoteEndPoint);

                await connection.SendAsync(Op, ResponseStatus.LOCKED).ConfigureAwait(false);
                return;
            }

            // Verify password
            if (!SecureCredentials.VerifyCredentialHash(
                    credentials.Password, account.Salt, account.Hash))
            {
                account.FailedLoginCount++;
                account.LastFailedLoginAt = DateTime.UtcNow;
                _ = await _accounts.SaveChangesAsync().ConfigureAwait(false);

                NLogix.Host.Instance.Warn(
                    "Incorrect password for {0}, attempt {1} from connection {2}",
                    credentials.Username, account.FailedLoginCount, connection.RemoteEndPoint);

                await connection.SendAsync(Op, ResponseStatus.INVALID_CREDENTIALS).ConfigureAwait(false);
                return;
            }

            // Disabled account
            if (!account.IsActive)
            {
                NLogix.Host.Instance.Warn(
                    "LOGIN attempt on disabled account {0} from connection {1}",
                    credentials.Username, connection.RemoteEndPoint);

                await connection.SendAsync(Op, ResponseStatus.DISABLED).ConfigureAwait(false);
                return;
            }

            // Reset counters and update last login
            account.FailedLoginCount = 0;
            account.LastFailedLoginAt = null;
            account.LastLoginAt = DateTime.UtcNow;
            _ = await _accounts.SaveChangesAsync().ConfigureAwait(false);

            // Update connection state
            connection.Level = account.Role;
            InstanceManager.Instance.GetOrCreateInstance<ConnectionHub>()
                                    .AssociateUsername(connection, account.Username);

            NLogix.Host.Instance.Info(
                "User {0} logged in successfully from connection {1}",
                credentials.Username, connection.RemoteEndPoint);

            await connection.SendAsync(Op, ResponseStatus.OK).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            NLogix.Host.Instance.Error(
                "LOGIN failed for {0} from connection {1}: {2}",
                credentials.Username, connection.RemoteEndPoint, ex.Message);

            await connection.SendAsync(Op, ResponseStatus.INTERNAL_ERROR).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Handles user logout.
    /// </summary>
    [PacketOpcode((UInt16)Command.LOGOUT)]
    [PacketPermission(PermissionLevel.User)]
    [PacketEncryption(false)]
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    internal async Task LogoutAsync(CredentialsPacket ___, IConnection connection)
    {
        const UInt16 Op = (UInt16)Command.LOGOUT;

        String username = InstanceManager.Instance
                                         .GetOrCreateInstance<ConnectionHub>()
                                         .GetUsername(connection.Id);

        if (username is null)
        {
            NLogix.Host.Instance.Warn(
                "LOGOUT attempt without valid session from connection {0}",
                connection.RemoteEndPoint);

            await connection.SendAsync(Op, ResponseStatus.INVALID_SESSION)
                            .ConfigureAwait(false);
            return;
        }

        try
        {
            Credentials account = await _accounts.GetFirstOrDefaultAsync(
                a => a.Username == username).ConfigureAwait(false);

            if (account is not null)
            {
                account.IsActive = false;
                account.LastLogoutAt = DateTime.UtcNow;
                _ = await _accounts.SaveChangesAsync().ConfigureAwait(false);
            }

            // Reset connection state
            connection.Level = PermissionLevel.Guest;
            _ = InstanceManager.Instance.GetOrCreateInstance<ConnectionHub>()
                                        .UnregisterConnection(connection.Id);

            NLogix.Host.Instance.Info(
                "User {0} logged out successfully from connection {1}",
                username, connection.RemoteEndPoint);

            // Send response first, then disconnect so client receives the packet
            await connection.SendAsync(Op, ResponseStatus.OK).ConfigureAwait(false);
            connection.Disconnect();
        }
        catch (Exception ex)
        {
            NLogix.Host.Instance.Error(
                "LOGOUT failed for {0} from connection {1}: {2}",
                username, connection.RemoteEndPoint, ex.Message);

            // Try to inform client about error, then disconnect
            await connection.SendAsync(Op, ResponseStatus.INTERNAL_ERROR).ConfigureAwait(false);

            connection.Level = PermissionLevel.Guest;
            connection.Disconnect();
        }
    }
}
