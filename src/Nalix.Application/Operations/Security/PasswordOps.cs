// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Nalix.Application.Validators;
using Nalix.Common.Connection;
using Nalix.Common.Enums;
using Nalix.Common.Packets.Abstractions;
using Nalix.Common.Packets.Attributes;
using Nalix.Common.Protocols;
using Nalix.Framework.Injection;
using Nalix.Infrastructure.Repositories;            // ControlType, ProtocolCode, ProtocolAction, ControlFlags
using Nalix.Logging;
using Nalix.Network.Connection;          // ConnectionExtensions.SendAsync
using Nalix.Protocol.Collections;
using Nalix.Protocol.Enums;
using Nalix.Shared.Security.Credentials;

namespace Nalix.Application.Operations.Security;

/// <summary>
/// Handles password change for authenticated users (Dapper-based).
/// Emits synchronized control directives via <see cref="ConnectionExtensions.SendAsync"/>.
/// </summary>
[PacketController]
public sealed class PasswordOps(CredentialsRepository accounts) : OpsBase
{
    private readonly CredentialsRepository _accounts = accounts ?? throw new System.ArgumentNullException(nameof(accounts));

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
            await SendErrorAsync(connection, seq, ProtocolCode.UNSUPPORTED_PACKET, ProtocolAction.DO_NOT_RETRY).ConfigureAwait(false);

            NLogix.Host.Instance.Debug(
                "Invalid packet type. Expected CredsUpdatePacket from {0}", connection.RemoteEndPoint);

            return;
        }

        // Resolve username from connection hub
        System.String username = InstanceManager.Instance.GetOrCreateInstance<ConnectionHub>()
                                                         .GetUsername(connection.ID);

        if (username is null)
        {
            await SendErrorAsync(connection, seq, ProtocolCode.SESSION_NOT_FOUND, ProtocolAction.DO_NOT_RETRY).ConfigureAwait(false);

            NLogix.Host.Instance.Debug(
                "CHANGE_PASSWORD attempt without valid session from {0}", connection.RemoteEndPoint);

            return;
        }

        if (packet.OldPassword is null || packet.NewPassword is null)
        {
            await SendErrorAsync(connection, seq, ProtocolCode.VALIDATION_FAILED, ProtocolAction.FIX_AND_RETRY).ConfigureAwait(false);

            NLogix.Host.Instance.Debug(
                "Null payload in CHANGE_PASSWORD from {0}", connection.RemoteEndPoint);

            return;
        }

        // Check new password strength
        if (!CredentialPolicy.IsStrongPassword(packet.NewPassword))
        {
            await SendErrorAsync(connection, seq, ProtocolCode.VALIDATION_FAILED, ProtocolAction.FIX_AND_RETRY).ConfigureAwait(false);
            return;
        }

        try
        {
            // 1) Minimal fetch for password change (Id, Salt, Hash, IsActive)
            var auth = await _accounts.GetForPasswordChangeByUsernameAsync(username).ConfigureAwait(false);

            if (auth is null)
            {
                await SendErrorAsync(connection, seq, ProtocolCode.SESSION_NOT_FOUND, ProtocolAction.DO_NOT_RETRY).ConfigureAwait(false);
                return;
            }

            var (id, salt, hash, isActive) = auth.Value;

            if (!isActive)
            {
                await SendErrorAsync(
                    connection, seq,
                    ProtocolCode.ACCOUNT_SUSPENDED,
                    ProtocolAction.DO_NOT_RETRY,
                    flags: ControlFlags.IS_AUTH_RELATED).ConfigureAwait(false);

                NLogix.Host.Instance.Debug(
                    "CHANGE_PASSWORD on disabled account {0} from {1}", username, connection.RemoteEndPoint);

                return;
            }

            // 2) Verify current password locally
            if (!Pbkdf2.Verify(packet.OldPassword, salt, hash))
            {
                await SendErrorAsync(
                        connection, seq,
                        ProtocolCode.UNAUTHENTICATED,
                        ProtocolAction.REAUTHENTICATE,
                        flags: ControlFlags.IS_AUTH_RELATED).ConfigureAwait(false);

                NLogix.Host.Instance.Debug(
                    "CHANGE_PASSWORD wrong current password for {0} from {1}", username, connection.RemoteEndPoint);

                return;
            }

            // 3) Hash new password
            Pbkdf2.Hash(packet.NewPassword, out System.Byte[] newSalt, out System.Byte[] newHash);

            // 4) Atomic update (match on old hash to avoid races)
            System.Int32 changed = await _accounts.UpdatePasswordIfMatchesAsync(id, hash, newSalt, newHash)
                                                  .ConfigureAwait(false);

            // 5) Clear sensitive buffers ASAP
            System.Array.Clear(salt, 0, salt.Length);
            System.Array.Clear(hash, 0, hash.Length);
            System.Array.Clear(newSalt, 0, newSalt.Length);
            System.Array.Clear(newHash, 0, newHash.Length);

            if (changed == 0)
            {
                await SendErrorAsync(
                        connection, seq,
                        ProtocolCode.VALIDATION_FAILED,
                        ProtocolAction.BACKOFF_RETRY,
                        flags: ControlFlags.IS_TRANSIENT).ConfigureAwait(false);

                NLogix.Host.Instance.Debug(
                    "CHANGE_PASSWORD concurrency mismatch for {0} from {1}", username, connection.RemoteEndPoint);

                return;
            }

            await SendAckAsync(connection, seq).ConfigureAwait(false);

            NLogix.Host.Instance.Debug(
                "Password changed successfully for {0} from {1}", username, connection.RemoteEndPoint);
        }
        catch (System.Exception ex)
        {
            await SendErrorAsync(
                connection, seq,
                ProtocolCode.INTERNAL_ERROR,
                ProtocolAction.BACKOFF_RETRY,
                flags: ControlFlags.IS_TRANSIENT).ConfigureAwait(false);

            NLogix.Host.Instance.Error(
                "CHANGE_PASSWORD failed for {0} from {1}: {2}", username, connection.RemoteEndPoint, ex.Message);
        }
    }
}
