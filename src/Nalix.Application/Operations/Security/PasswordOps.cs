// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Nalix.Common.Connection;
using Nalix.Common.Enums;
using Nalix.Common.Packets.Abstractions;
using Nalix.Common.Packets.Attributes;
using Nalix.Communication.Collections;
using Nalix.Communication.Enums;
using Nalix.Communication.Models;
using Nalix.Framework.Injection;
using Nalix.Infrastructure.Abstractions;
using Nalix.Logging;
using Nalix.Network.Connection;          // ConnectionExtensions.SendAsync
using Nalix.Common.Protocols;
using Nalix.Framework.Cryptography.Security;            // ControlType, ProtocolCode, ProtocolAction, ControlFlags

namespace Nalix.Application.Operations.Security;

/// <summary>
/// Handles password change for authenticated users (Dapper-based).
/// Emits synchronized control directives via <see cref="ConnectionExtensions.SendAsync"/>.
/// </summary>
[PacketController]
public sealed class PasswordOps(ICredentialsRepository accounts) : OpsBase
{
    private readonly ICredentialsRepository _accounts = accounts ?? throw new System.ArgumentNullException(nameof(accounts));

    /// <summary>
    /// Change the current user's password:
    /// - Validate session & payload
    /// - Verify current password
    /// - Check new password strength
    /// - Re-hash and persist (salt + hash)
    /// - Clear sensitive buffers
    /// </summary>
    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.User)]
    [PacketOpcode((System.UInt16)OpCommand.CHANGE_PASSWORD)]
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public async System.Threading.Tasks.Task ChangePasswordAsync(IPacket p, IConnection connection)
    {
        System.ArgumentNullException.ThrowIfNull(connection);
        System.UInt32 seq = GetSequenceIdOrZero(p);

        if (p is not CredsUpdatePacket packet)
        {
            NLogix.Host.Instance.Error(
                "Invalid packet type. Expected CredsUpdatePacket from {0}",
                connection.RemoteEndPoint);

            await SendErrorAsync(connection, seq, ProtocolCode.UNSUPPORTED_PACKET, ProtocolAction.DO_NOT_RETRY)
                .ConfigureAwait(false);
            return;
        }

        // Resolve username from connection hub
        System.String username = InstanceManager.Instance.GetOrCreateInstance<ConnectionHub>()
                                                         .GetUsername(connection.ID);

        if (username is null)
        {
            NLogix.Host.Instance.Warn(
                "CHANGE_PASSWORD attempt without valid session from {0}",
                connection.RemoteEndPoint);

            await SendErrorAsync(connection, seq, ProtocolCode.SESSION_NOT_FOUND, ProtocolAction.DO_NOT_RETRY)
                .ConfigureAwait(false);
            return;
        }

        if (packet.OldPassword is null || packet.NewPassword is null)
        {
            NLogix.Host.Instance.Error(
                "Null payload in CHANGE_PASSWORD from {0}",
                connection.RemoteEndPoint);

            await SendErrorAsync(connection, seq, ProtocolCode.VALIDATION_FAILED, ProtocolAction.FIX_AND_RETRY)
                .ConfigureAwait(false);
            return;
        }

        try
        {
            // Fetch account by username (Dapper)
            Credentials account = await _accounts.GetByUsernameAsync(username).ConfigureAwait(false);

            if (account is null)
            {
                await SendErrorAsync(connection, seq, ProtocolCode.SESSION_NOT_FOUND, ProtocolAction.DO_NOT_RETRY)
                    .ConfigureAwait(false);
                return;
            }

            if (!account.IsActive)
            {
                NLogix.Host.Instance.Warn(
                    "CHANGE_PASSWORD on disabled account {0} from {1}",
                    username, connection.RemoteEndPoint);

                await SendErrorAsync(
                        connection, seq,
                        ProtocolCode.ACCOUNT_SUSPENDED,
                        ProtocolAction.DO_NOT_RETRY,
                        flags: ControlFlags.IS_AUTH_RELATED)
                    .ConfigureAwait(false);
                return;
            }

            // Verify old password
            if (!SecureCredentials.VerifyCredentialHash(packet.OldPassword, account.Salt, account.Hash))
            {
                NLogix.Host.Instance.Warn(
                    "CHANGE_PASSWORD wrong current password for {0} from {1}",
                    username, connection.RemoteEndPoint);

                await SendErrorAsync(
                        connection, seq,
                        ProtocolCode.UNAUTHENTICATED,
                        ProtocolAction.REAUTHENTICATE,
                        flags: ControlFlags.IS_AUTH_RELATED)
                    .ConfigureAwait(false);
                return;
            }

            // Check new password strength
            if (!IsStrongPassword(packet.NewPassword))
            {
                await SendErrorAsync(connection, seq, ProtocolCode.VALIDATION_FAILED, ProtocolAction.FIX_AND_RETRY)
                    .ConfigureAwait(false);
                return;
            }

            // Generate new salt + hash
            SecureCredentials.GenerateCredentialHash(
                packet.NewPassword,
                out System.Byte[] newSalt,
                out System.Byte[] newHash);

            // Persist
            account.Salt = newSalt;
            account.Hash = newHash;
            _ = await _accounts.UpdateAsync(account).ConfigureAwait(false);

            // Clear sensitive buffers
            System.Array.Clear(newSalt, 0, newSalt.Length);
            System.Array.Clear(newHash, 0, newHash.Length);

            NLogix.Host.Instance.Info(
                "Password changed successfully for {0} from {1}",
                username, connection.RemoteEndPoint);

            await SendAckAsync(connection, seq).ConfigureAwait(false);
        }
        catch (System.Exception ex)
        {
            NLogix.Host.Instance.Error(
                "CHANGE_PASSWORD failed for {0} from {1}: {2}",
                username, connection.RemoteEndPoint, ex.Message);

            await SendErrorAsync(
                    connection, seq,
                    ProtocolCode.INTERNAL_ERROR,
                    ProtocolAction.BACKOFF_RETRY,
                    flags: ControlFlags.IS_TRANSIENT)
                .ConfigureAwait(false);
        }
    }

    /// <summary>Very simple password strength check. Replace with your policy if needed.</summary>
    private static System.Boolean IsStrongPassword(System.String pwd)
    {
        if (System.String.IsNullOrWhiteSpace(pwd) || pwd.Length < 8)
        {
            return false;
        }

        System.Boolean hasLetter = false, hasDigit = false;
        foreach (System.Char c in pwd)
        {
            if (System.Char.IsLetter(c))
            {
                hasLetter = true;
            }
            else if (System.Char.IsDigit(c))
            {
                hasDigit = true;
            }

            if (hasLetter && hasDigit)
            {
                return true;
            }
        }
        return false;
    }
}
