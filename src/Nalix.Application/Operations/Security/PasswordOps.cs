using Nalix.Application.Extensions;
using Nalix.Common.Connection;
using Nalix.Common.Packets.Abstractions;
using Nalix.Common.Packets.Attributes;
using Nalix.Common.Security.Types;
using Nalix.Communication.Collections;
using Nalix.Communication.Enums;
using Nalix.Communication.Models;
using Nalix.Cryptography.Security;
using Nalix.Infrastructure.Database;
using Nalix.Infrastructure.Repositories;
using Nalix.Logging;
using Nalix.Network.Connection;
using Nalix.Shared.Injection;

namespace Nalix.Application.Operations.Security;

/// <summary>
/// Handles password change for authenticated users.
/// Requires the user to provide the current password and a new strong password.
/// </summary>
[PacketController]
public sealed class PasswordOps
{
    private readonly Repository<Credentials> _accounts;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "<Pending>")]
    public PasswordOps(GameDbContext context) => _accounts = new Repository<Credentials>(context);

    /// <summary>
    /// Change the current user's password:
    /// - Validate session and payload,
    /// - Verify current password,
    /// - Check new password strength,
    /// - Re-hash and persist (salt + hash),
    /// - Clear sensitive buffers.
    /// </summary>
    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.User)]
    [PacketOpcode((System.UInt16)Command.CHANGE_PASSWORD)]
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    internal async System.Threading.Tasks.Task ChangePasswordAsync(IPacket p, IConnection connection)
    {
        const System.UInt16 Op = (System.UInt16)Command.CHANGE_PASSWORD;

        if (p is not CredsUpdatePacket packet)
        {
            NLogix.Host.Instance.Error(
                "Invalid packet type. Expected HandshakePacket from {0}",
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

            await connection.SendAsync(Op, ResponseStatus.INVALID_SESSION)
                            .ConfigureAwait(false);
            return;
        }

        if (packet is null || packet.OldPassword is null || packet.NewPassword is null)
        {
            NLogix.Host.Instance.Error(
                "Null payload in CHANGE_PASSWORD from {0}",
                connection.RemoteEndPoint);

            await connection.SendAsync(Op, ResponseStatus.INVALID_PAYLOAD)
                            .ConfigureAwait(false);
            return;
        }

        try
        {
            Credentials account = await _accounts.GetFirstOrDefaultAsync(a => a.Username == username)
                                                 .ConfigureAwait(false);

            if (account is null)
            {
                // Session có username nhưng DB không còn — xem như invalid
                await connection.SendAsync(Op, ResponseStatus.INVALID_SESSION)
                                .ConfigureAwait(false);
                return;
            }

            if (!account.IsActive)
            {
                NLogix.Host.Instance.Warn(
                    "CHANGE_PASSWORD on disabled account {0} from {1}",
                    username, connection.RemoteEndPoint);

                await connection.SendAsync(Op, ResponseStatus.DISABLED)
                                .ConfigureAwait(false);
                return;
            }

            // Xác thực mật khẩu cũ
            if (!SecureCredentials.VerifyCredentialHash(packet.OldPassword, account.Salt, account.Hash))
            {
                NLogix.Host.Instance.Warn(
                    "CHANGE_PASSWORD wrong current password for {0} from {1}",
                    username, connection.RemoteEndPoint);

                await connection.SendAsync(Op, ResponseStatus.INVALID_CREDENTIALS)
                                .ConfigureAwait(false);
                return;
            }

            // Kiểm tra độ mạnh mật khẩu mới (tối thiểu ví dụ: >= 8, có chữ/số)
            if (!IsStrongPassword(packet.NewPassword))
            {
                await connection.SendAsync(Op, ResponseStatus.PASSWORD_TOO_WEAK)
                                .ConfigureAwait(false);
                return;
            }

            // Tạo salt + hash mới
            SecureCredentials.GenerateCredentialHash(
                packet.NewPassword,
                out System.Byte[] newSalt,
                out System.Byte[] newHash);

            // Cập nhật DB
            account.Salt = newSalt;
            account.Hash = newHash;
            _ = await _accounts.SaveChangesAsync().ConfigureAwait(false);

            // Dọn dẹp nhạy cảm (old/new password chuỗi thì .NET GC quản, còn mảng clear)
            System.Array.Clear(newSalt, 0, newSalt.Length);
            System.Array.Clear(newHash, 0, newHash.Length);

            NLogix.Host.Instance.Info(
                "Password changed successfully for {0} from {1}",
                username, connection.RemoteEndPoint);

            await connection.SendAsync(Op, ResponseStatus.OK)
                            .ConfigureAwait(false);
        }
        catch (System.Exception ex)
        {
            NLogix.Host.Instance.Error(
                "CHANGE_PASSWORD failed for {0} from {1}: {2}",
                username, connection.RemoteEndPoint, ex.Message);

            await connection.SendAsync(Op, ResponseStatus.INTERNAL_ERROR)
                            .ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Very simple password strength check. Replace with your policy if needed.
    /// </summary>
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
