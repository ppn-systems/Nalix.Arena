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
using Nalix.Network.Connection;

namespace Nalix.Application.Operations.Security;

/// <summary>
/// Handles password change for authenticated users (Dapper-based).
/// Requires current password and a new strong password.
/// </summary>
[PacketController]
public sealed class PasswordOps
{
    private readonly ICredentialsRepository _accounts;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "<Pending>")]
    public PasswordOps(ICredentialsRepository accounts)
        => _accounts = accounts ?? throw new System.ArgumentNullException(nameof(accounts));

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
        const System.UInt16 Op = (System.UInt16)OpCommand.CHANGE_PASSWORD;

        if (p is not CredsUpdatePacket packet)
        {
            NLogix.Host.Instance.Error(
                "Invalid packet type. Expected CredsUpdatePacket from {0}",
                connection.RemoteEndPoint);

            await connection.SendAsync(Op, ResponseStatus.INVALID_PACKET).ConfigureAwait(false);
            return;
        }

        // Lấy username từ hub theo connection
        System.String username = InstanceManager.Instance.GetOrCreateInstance<ConnectionHub>()
                                                  .GetUsername(connection.ID);

        if (username is null)
        {
            NLogix.Host.Instance.Warn(
                "CHANGE_PASSWORD attempt without valid session from {0}",
                connection.RemoteEndPoint);

            await connection.SendAsync(Op, ResponseStatus.INVALID_SESSION).ConfigureAwait(false);
            return;
        }

        if (packet.OldPassword is null || packet.NewPassword is null)
        {
            NLogix.Host.Instance.Error(
                "Null payload in CHANGE_PASSWORD from {0}",
                connection.RemoteEndPoint);

            await connection.SendAsync(Op, ResponseStatus.INVALID_PAYLOAD).ConfigureAwait(false);
            return;
        }

        try
        {
            // Dapper: lấy tài khoản theo username
            Credentials account = await _accounts.GetByUsernameAsync(username).ConfigureAwait(false);

            if (account is null)
            {
                await connection.SendAsync(Op, ResponseStatus.INVALID_SESSION).ConfigureAwait(false);
                return;
            }

            if (!account.IsActive)
            {
                NLogix.Host.Instance.Warn(
                    "CHANGE_PASSWORD on disabled account {0} from {1}",
                    username, connection.RemoteEndPoint);

                await connection.SendAsync(Op, ResponseStatus.DISABLED).ConfigureAwait(false);
                return;
            }

            // Xác thực mật khẩu cũ
            if (!SecureCredentials.VerifyCredentialHash(packet.OldPassword, account.Salt, account.Hash))
            {
                NLogix.Host.Instance.Warn(
                    "CHANGE_PASSWORD wrong current password for {0} from {1}",
                    username, connection.RemoteEndPoint);

                await connection.SendAsync(Op, ResponseStatus.INVALID_CREDENTIALS).ConfigureAwait(false);
                return;
            }

            // Kiểm tra độ mạnh mật khẩu mới
            if (!IsStrongPassword(packet.NewPassword))
            {
                await connection.SendAsync(Op, ResponseStatus.PASSWORD_TOO_WEAK).ConfigureAwait(false);
                return;
            }

            // Tạo salt + hash mới
            SecureCredentials.GenerateCredentialHash(
                packet.NewPassword,
                out System.Byte[] newSalt,
                out System.Byte[] newHash);

            // Cập nhật DB qua Dapper repo
            account.Salt = newSalt;
            account.Hash = newHash;
            _ = await _accounts.UpdateAsync(account).ConfigureAwait(false);

            // Dọn dẹp nhạy cảm
            System.Array.Clear(newSalt, 0, newSalt.Length);
            System.Array.Clear(newHash, 0, newHash.Length);

            NLogix.Host.Instance.Info(
                "Password changed successfully for {0} from {1}",
                username, connection.RemoteEndPoint);

            await connection.SendAsync(Op, ResponseStatus.OK).ConfigureAwait(false);
        }
        catch (System.Exception ex)
        {
            NLogix.Host.Instance.Error(
                "CHANGE_PASSWORD failed for {0} from {1}: {2}",
                username, connection.RemoteEndPoint, ex.Message);

            await connection.SendAsync(Op, ResponseStatus.INTERNAL_ERROR).ConfigureAwait(false);
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
