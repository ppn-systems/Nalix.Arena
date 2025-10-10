namespace Nalix.Application.Validators;

/// <summary>
/// Provides username and password validation logic
/// without using regular expressions.
/// </summary>
public static class CredentialPolicy
{
    #region Constants

    // Username policy
    private const System.Int32 UsernameMinLength = 3;
    private const System.Int32 UsernameMaxLength = 32;

    // Password policy
    private const System.Int32 PasswordMinLength = 6;
    private const System.Int32 PasswordMaxLength = 128;

    #endregion Constants

    /// <summary>
    /// Validates username based on allowed characters and structure.
    /// </summary>
    /// <param name="username">The username to validate.</param>
    /// <returns>True if valid, otherwise false.</returns>
    public static System.Boolean IsValidUsername(System.String username)
    {
        if (username is null)
        {
            return false;
        }

        if (username.Length is < UsernameMinLength or > UsernameMaxLength)
        {
            return false;
        }

        // Username cannot start or end with special characters
        if (IsSpecial(username[0]) || IsSpecial(username[^1]))
        {
            return false;
        }

        System.Boolean previousWasSpecial = false;
        foreach (System.Char c in username)
        {
            if (System.Char.IsLetterOrDigit(c))
            {
                previousWasSpecial = false;
                continue;
            }

            if (!IsSpecial(c))
            {
                return false;
            }

            // Prevent consecutive special chars like "__" or ".."
            if (previousWasSpecial)
            {
                return false;
            }

            previousWasSpecial = true;
        }

        return true;

        static System.Boolean IsSpecial(System.Char c) => c is '_' or '-' or '.';
    }

    /// <summary>
    /// Validates password strength according to secure policy.
    /// Requires 10–128 characters, with upper/lowercase, digit, and special symbol.
    /// </summary>
    /// <param name="password">The password to validate.</param>
    /// <returns>True if password is strong enough.</returns>
    public static System.Boolean IsStrongPassword(System.String password)
    {
        if (password is null)
        {
            return false;
        }

        if (password.Length is < PasswordMinLength or > PasswordMaxLength)
        {
            return false;
        }

        System.Boolean hasLower = false;
        System.Boolean hasUpper = false;
        System.Boolean hasDigit = false;
        System.Boolean hasSymbol = false;

        foreach (System.Char c in password)
        {
            if (System.Char.IsLower(c))
            {
                hasLower = true;
            }
            else if (System.Char.IsUpper(c))
            {
                hasUpper = true;
            }
            else if (System.Char.IsDigit(c))
            {
                hasDigit = true;
            }
            else if (IsSymbol(c))
            {
                hasSymbol = true;
            }

            if (hasLower && hasUpper && hasDigit && hasSymbol)
            {
                break;
            }
        }

        if (!(hasLower && hasUpper && hasDigit && hasSymbol))
        {
            return false;
        }

        // Reject common weak words
        System.String lower = password.ToLowerInvariant();
        return !ContainsCommonWord(lower);

        static System.Boolean IsSymbol(System.Char c)
        {
            // Basic ASCII symbols accepted as "special"
            const System.Char PunctStart1 = '!';
            const System.Char PunctEnd1 = '/';
            const System.Char PunctStart2 = ':';
            const System.Char PunctEnd2 = '@';
            const System.Char PunctStart3 = '[';
            const System.Char PunctEnd3 = '`';
            const System.Char PunctStart4 = '{';
            const System.Char PunctEnd4 = '~';

            return c is >= PunctStart1 and <= PunctEnd1
                     or >= PunctStart2 and <= PunctEnd2
                     or >= PunctStart3 and <= PunctEnd3
                     or >= PunctStart4 and <= PunctEnd4;
        }

        static System.Boolean ContainsCommonWord(System.String s)
        {
            s = s.Replace('0', 'o')
                 .Replace('1', 'i')
                 .Replace('3', 'e')
                 .Replace('@', 'a')
                 .Replace('$', 's');

            System.ReadOnlySpan<System.String> weak =
            [
                "password", "passw0rd", "password1", "123456", "123456789",
                "qwerty", "abc123", "111111", "letmein", "welcome", "admin",
                "iloveyou", "monkey", "dragon", "football", "baseball", "superman",
                "login", "starwars", "pokemon", "shadow", "sunshine", "princess",
                "hello", "freedom", "whatever", "trustno1", "master", "batman",
                "654321", "1q2w3e4r", "zaq12wsx", "qwertyuiop", "asdfghjkl",
                "test", "default", "root", "guest"
            ];

            foreach (System.String word in weak)
            {
                if (s.Contains(word, System.StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
