using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Nalix.Game.Shared.Security;

public class PasswordChange
{
    /// <summary>
    /// The old password to be changed.
    /// </summary>
    public string OldPassword { get; private set; } = string.Empty;

    /// <summary>
    /// The new password to be set.
    /// </summary>
    public string NewPassword { get; private set; } = string.Empty;

    /// <summary>
    /// Parses the given byte array into a PasswordChange object.
    /// </summary>
    public static bool TryParse(
        ReadOnlySpan<byte> data,
        [NotNullWhen(true)] out PasswordChange result)
    {
        result = null;

        try
        {
            int offset = 0;

            // Old Password
            if (offset >= data.Length) return false;
            byte oldPassLen = data[offset++];
            if (offset + oldPassLen > data.Length) return false;
            string oldPassword = Encoding.UTF8.GetString(data.Slice(offset, oldPassLen));
            offset += oldPassLen;

            // New Password
            if (offset >= data.Length) return false;
            byte newPassLen = data[offset++];
            if (offset + newPassLen > data.Length) return false;
            string newPassword = Encoding.UTF8.GetString(data.Slice(offset, newPassLen));

            result = new PasswordChange
            {
                OldPassword = oldPassword,
                NewPassword = newPassword
            };

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Builds a byte array from the old and new passwords.
    /// </summary>
    public byte[] Build()
    {
        byte[] oldPassBytes = Encoding.UTF8.GetBytes(OldPassword);
        byte[] newPassBytes = Encoding.UTF8.GetBytes(NewPassword);

        if (oldPassBytes.Length > Credentials.PasswordMaxLength)
            throw new ArgumentException("Old password is too long.");
        if (newPassBytes.Length > Credentials.PasswordMaxLength)
            throw new ArgumentException("New password is too long.");

        byte[] buffer = new byte[1 + oldPassBytes.Length + 1 + newPassBytes.Length];
        int offset = 0;

        buffer[offset++] = (byte)oldPassBytes.Length;
        oldPassBytes.CopyTo(buffer.AsSpan(offset));
        offset += oldPassBytes.Length;

        buffer[offset++] = (byte)newPassBytes.Length;
        newPassBytes.CopyTo(buffer.AsSpan(offset));

        return buffer;
    }
}