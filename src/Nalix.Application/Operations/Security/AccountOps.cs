// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Nalix.Application.Extensions;
using Nalix.Common.Connection;
using Nalix.Common.Enums;
using Nalix.Common.Packets.Abstractions;
using Nalix.Common.Packets.Attributes;
using Nalix.Communication.Collections;
using Nalix.Communication.Enums;
using Nalix.Communication.Models;
using Nalix.Cryptography.Security;
using Nalix.Framework.Injection;
using Nalix.Infrastructure.Abstractions;
using Nalix.Logging;
using Nalix.Network.Connection;                 // <-- for ConnectionExtensions.SendAsync(..)
using Nalix.Shared.Memory.Pooling;
using Nalix.Common.Protocols;                   // <-- ControlType / ProtocolCode / ProtocolAction / ControlFlags

namespace Nalix.Application.Operations.Security;

/// <summary>
/// User account management service: register, login, logout with secure practices and pooling (Dapper-based).
/// Now emits synchronized control directives via <see cref="ConnectionExtensions.SendAsync"/>.
/// </summary>
[PacketController]
public sealed class AccountOps
{
    private readonly ICredentialsRepository _accounts;

    static AccountOps()
    {
        _ = InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>()
                                    .SetMaxCapacity<CredentialsPacket>(1024);

        _ = InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>()
                                    .Prealloc<CredentialsPacket>(128);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Style", "IDE0290:Use primary constructor", Justification = "<Pending>")]
    public AccountOps(ICredentialsRepository accounts)
        => _accounts = accounts ?? throw new System.ArgumentNullException(nameof(accounts));

    #region Helpers

    /// <summary>
    /// Attempts to get a correlation SequenceId from the packet, falls back to 0 if not present.
    /// </summary>
    private static System.UInt32 GetSequenceIdOrZero(IPacket p)
    {
        if (p is IPacketSequenced seq)
        {
            return seq.SequenceId;
        }
        return 0u;
    }

    private static System.Threading.Tasks.Task SendAckAsync(IConnection c, System.UInt32 seq)
        => c.SendAsync(ControlType.ACK, ProtocolCode.NONE, ProtocolAction.NONE, sequenceId: seq);

    private static System.Threading.Tasks.Task SendErrorAsync(
        IConnection c, System.UInt32 seq, ProtocolCode code, ProtocolAction action,
        ControlFlags flags = ControlFlags.NONE)
        => c.SendAsync(ControlType.ERROR, code, action, sequenceId: seq, flags: flags);

    #endregion

    /// <summary>
    /// Handles user registration.
    /// </summary>
    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.Guest)]
    [PacketOpcode((System.UInt16)OpCommand.REGISTER)]
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public async System.Threading.Tasks.Task RegisterAsync(IPacket p, IConnection connection)
    {
        System.ArgumentNullException.ThrowIfNull(connection);
        System.UInt32 seq = GetSequenceIdOrZero(p);

        if (p is not CredentialsPacket packet)
        {
            NLogix.Host.Instance.Error(
                "Invalid packet type. Expected CredentialsPacket from {0}",
                connection.RemoteEndPoint);

            await SendErrorAsync(connection, seq, ProtocolCode.UNSUPPORTED_PACKET, ProtocolAction.DO_NOT_RETRY)
                .ConfigureAwait(false);
            return;
        }

        if (packet.Credentials is null)
        {
            NLogix.Host.Instance.Error(
                "Null credentials in register packet from {0}",
                connection.RemoteEndPoint);

            await SendErrorAsync(connection, seq, ProtocolCode.VALIDATION_FAILED, ProtocolAction.FIX_AND_RETRY)
                .ConfigureAwait(false);
            return;
        }

        Credentials credentials = packet.Credentials;

        // Basic input validation
        if (System.String.IsNullOrWhiteSpace(credentials.Username) ||
            System.String.IsNullOrWhiteSpace(credentials.Password))
        {
            NLogix.Host.Instance.Debug(
                "Empty username or password in register attempt from {0}",
                connection.RemoteEndPoint);

            await SendErrorAsync(connection, seq, ProtocolCode.VALIDATION_FAILED, ProtocolAction.FIX_AND_RETRY)
                .ConfigureAwait(false);
            return;
        }

        try
        {
            // Check existing username (Dapper)
            var existed = await _accounts.GetByUsernameAsync(credentials.Username).ConfigureAwait(false);
            if (existed is not null)
            {
                NLogix.Host.Instance.Warn(
                    "Username {0} already exists from connection {1}",
                    credentials.Username, connection.RemoteEndPoint);

                await SendErrorAsync(connection, seq, ProtocolCode.ALREADY_EXISTS, ProtocolAction.FIX_AND_RETRY)
                    .ConfigureAwait(false);
                return;
            }

            // Derive salt/hash
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

            _ = await _accounts.InsertAsync(newAccount).ConfigureAwait(false);

            // Clear sensitive
            System.Array.Clear(salt, 0, salt.Length);
            System.Array.Clear(hash, 0, hash.Length);

            NLogix.Host.Instance.Info(
                "Account {0} registered successfully from connection {1}",
                credentials.Username, connection.RemoteEndPoint);

            await SendAckAsync(connection, seq).ConfigureAwait(false);
        }
        catch (System.Exception ex)
        {
            NLogix.Host.Instance.Error(
                "Failed to register account {0} from connection {1}: {2}",
                credentials.Username, connection.RemoteEndPoint, ex.Message);

            await SendErrorAsync(
                    connection, seq,
                    ProtocolCode.INTERNAL_ERROR,
                    ProtocolAction.BACKOFF_RETRY,
                    flags: ControlFlags.IS_TRANSIENT)
                .ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Handles user login.
    /// </summary>
    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.Guest)]
    [PacketOpcode((System.UInt16)OpCommand.LOGIN)]
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public async System.Threading.Tasks.Task LoginAsync(IPacket p, IConnection connection)
    {
        System.ArgumentNullException.ThrowIfNull(connection);
        System.UInt32 seq = GetSequenceIdOrZero(p);

        if (p is not CredentialsPacket packet)
        {
            NLogix.Host.Instance.Error(
                "Invalid packet type. Expected CredentialsPacket from {0}",
                connection.RemoteEndPoint);

            await SendErrorAsync(connection, seq, ProtocolCode.UNSUPPORTED_PACKET, ProtocolAction.DO_NOT_RETRY)
                .ConfigureAwait(false);
            return;
        }

        if (packet.Credentials is null)
        {
            NLogix.Host.Instance.Error(
                "Null credentials in login packet from {0}",
                connection.RemoteEndPoint);

            await SendErrorAsync(connection, seq, ProtocolCode.VALIDATION_FAILED, ProtocolAction.FIX_AND_RETRY)
                .ConfigureAwait(false);
            return;
        }

        Credentials credentials = packet.Credentials;

        try
        {
            // Look up account (Dapper)
            Credentials account = await _accounts.GetByUsernameAsync(credentials.Username).ConfigureAwait(false);

            if (account is null)
            {
                NLogix.Host.Instance.Warn(
                    "LOGIN attempt with non-existent username {0} from connection {1}",
                    credentials.Username, connection.RemoteEndPoint);

                await SendErrorAsync(
                        connection, seq,
                        ProtocolCode.UNAUTHENTICATED,
                        ProtocolAction.REAUTHENTICATE,
                        flags: ControlFlags.IS_AUTH_RELATED)
                    .ConfigureAwait(false);
                return;
            }

            // Lockout window
            if (account.FailedLoginCount >= 5 &&
                account.LastFailedLoginAt.HasValue &&
                System.DateTime.UtcNow < account.LastFailedLoginAt.Value.AddMinutes(15))
            {
                NLogix.Host.Instance.Warn(
                    "Account {0} locked due to too many failed attempts from connection {1}",
                    credentials.Username, connection.RemoteEndPoint);

                await SendErrorAsync(
                        connection, seq,
                        ProtocolCode.ACCOUNT_LOCKED,
                        ProtocolAction.BACKOFF_RETRY,
                        flags: ControlFlags.IS_AUTH_RELATED)
                    .ConfigureAwait(false);
                return;
            }

            // Verify password
            if (!SecureCredentials.VerifyCredentialHash(credentials.Password, account.Salt, account.Hash))
            {
                account.FailedLoginCount++;
                account.LastFailedLoginAt = System.DateTime.UtcNow;
                _ = await _accounts.UpdateAsync(account).ConfigureAwait(false);

                NLogix.Host.Instance.Warn(
                    "Incorrect password for {0}, attempt {1} from connection {2}",
                    credentials.Username, account.FailedLoginCount, connection.RemoteEndPoint);

                await SendErrorAsync(
                        connection, seq,
                        ProtocolCode.UNAUTHENTICATED,
                        ProtocolAction.REAUTHENTICATE,
                        flags: ControlFlags.IS_AUTH_RELATED)
                    .ConfigureAwait(false);
                return;
            }

            // Disabled account
            if (!account.IsActive)
            {
                NLogix.Host.Instance.Warn(
                    "LOGIN attempt on disabled account {0} from connection {1}",
                    credentials.Username, connection.RemoteEndPoint);

                await SendErrorAsync(
                        connection, seq,
                        ProtocolCode.ACCOUNT_SUSPENDED,
                        ProtocolAction.DO_NOT_RETRY,
                        flags: ControlFlags.IS_AUTH_RELATED)
                    .ConfigureAwait(false);
                return;
            }

            // Reset counters and update last login
            account.FailedLoginCount = 0;
            account.LastFailedLoginAt = null;
            account.LastLoginAt = System.DateTime.UtcNow;
            _ = await _accounts.UpdateAsync(account).ConfigureAwait(false);

            // Update connection state
            connection.Level = account.Role;
            InstanceManager.Instance.GetOrCreateInstance<ConnectionHub>()
                                    .AssociateUsername(connection, account.Username);

            NLogix.Host.Instance.Info(
                "User {0} logged in successfully from connection {1}",
                credentials.Username, connection.RemoteEndPoint);

            await SendAckAsync(connection, seq).ConfigureAwait(false);
        }
        catch (System.Exception ex)
        {
            NLogix.Host.Instance.Error(
                "LOGIN failed for {0} from connection {1}: {2}",
                credentials.Username, connection.RemoteEndPoint, ex.Message);

            await SendErrorAsync(
                    connection, seq,
                    ProtocolCode.INTERNAL_ERROR,
                    ProtocolAction.BACKOFF_RETRY,
                    flags: ControlFlags.IS_TRANSIENT)
                .ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Handles user logout.
    /// </summary>
    [PacketEncryption(false)]
    [PacketPermission(PermissionLevel.User)]
    [PacketOpcode((System.UInt16)OpCommand.LOGOUT)]
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public async System.Threading.Tasks.Task LogoutAsync(IPacket p, IConnection connection)
    {
        System.ArgumentNullException.ThrowIfNull(p);
        System.ArgumentNullException.ThrowIfNull(connection);

        System.UInt32 seq = GetSequenceIdOrZero(p);

        System.String username = InstanceManager.Instance.GetOrCreateInstance<ConnectionHub>()
                                                 .GetUsername(connection.ID);

        if (username is null)
        {
            NLogix.Host.Instance.Warn(
                "LOGOUT attempt without valid session from connection {0}",
                connection.RemoteEndPoint);

            await SendErrorAsync(connection, seq, ProtocolCode.SESSION_NOT_FOUND, ProtocolAction.DO_NOT_RETRY)
                .ConfigureAwait(false);
            return;
        }

        try
        {
            Credentials account = await _accounts.GetByUsernameAsync(username).ConfigureAwait(false);

            if (account is not null)
            {
                // NOTE: Typically you do NOT deactivate account on logout.
                // Keep IsActive as-is; just stamp LastLogoutAt.
                account.LastLogoutAt = System.DateTime.UtcNow;
                _ = await _accounts.UpdateAsync(account).ConfigureAwait(false);
            }

            // Reset connection state
            connection.Level = PermissionLevel.Guest;
            _ = InstanceManager.Instance.GetOrCreateInstance<ConnectionHub>()
                                        .UnregisterConnection(connection);

            NLogix.Host.Instance.Info(
                "User {0} logged out successfully from connection {1}",
                username, connection.RemoteEndPoint);

            // Inform client to close (correlated), then disconnect so client receives it
            await connection.SendAsync(
                    ControlType.DISCONNECT,
                    ProtocolCode.CLIENT_QUIT,
                    ProtocolAction.NONE,
                    sequenceId: seq)
                .ConfigureAwait(false);

            connection.Disconnect();
        }
        catch (System.Exception ex)
        {
            NLogix.Host.Instance.Error(
                "LOGOUT failed for {0} from connection {1}: {2}",
                username, connection.RemoteEndPoint, ex.Message);

            // Best-effort error report then drop
            await SendErrorAsync(
                    connection, seq,
                    ProtocolCode.INTERNAL_ERROR,
                    ProtocolAction.BACKOFF_RETRY,
                    flags: ControlFlags.IS_TRANSIENT)
                .ConfigureAwait(false);

            connection.Level = PermissionLevel.Guest;
            connection.Disconnect();
        }
    }
}
