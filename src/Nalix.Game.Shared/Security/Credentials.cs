using Nalix.Common.Security;
using Nalix.Common.Serialization;
using Nalix.Common.Serialization.Attributes;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Nalix.Game.Shared.Security;

/// <summary>
/// Đại diện cho thông tin đăng nhập gồm tên người dùng và mật khẩu.
/// Có cấu hình kiểm tra độ dài tối thiểu và tối đa của username và password.
/// </summary>
[Table("Account")]
[SerializePackable(SerializeLayout.Explicit)]
public sealed class Credentials
{
    #region Constants

    public const int UsernameMinLength = 3;
    public const int UsernameMaxLength = 20;

    public const int PasswordMinLength = 8;
    public const int PasswordMaxLength = 50;

    #endregion Constants

    #region Properties

    /// <summary>
    /// ID của tài khoản trong cơ sở dữ liệu.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; private set; }

    /// <summary>
    /// Tên người dùng.
    /// </summary>
    [Column("Username")]
    [Required]
    [MinLength(3)]
    [MaxLength(20)]
    [RegularExpression(@"^[a-zA-Z0-9_-]+$",
        ErrorMessage = "Username can only contain letters, numbers, underscores, and hyphens.")]
    [SerializeOrder(0)]
    public string Username { get; set; }

    /// <summary>
    /// Mật khẩu (chỉ dùng trong code, không lưu vào database).
    /// </summary>
    [NotMapped]
    [SerializeOrder(1)]
    public string Password { get; set; }

    /// <summary>
    /// Chuỗi salt ngẫu nhiên được tạo ra để băm mật khẩu.
    /// Salt giúp bảo vệ mật khẩu khỏi các cuộc tấn công từ điển và rainbow table.
    /// </summary>
    [Required]
    [MaxLength(64)]
    [Column(TypeName = "binary(64)")]
    public byte[] Salt { get; set; }

    /// <summary>
    /// Mật khẩu sau khi được băm bằng thuật toán PBKDF2.
    /// Giá trị này được lưu trữ trong cơ sở dữ liệu để xác minh mật khẩu khi đăng nhập.
    /// </summary>
    [Required]
    [MaxLength(64)]
    [Column(TypeName = "binary(64)")]
    public byte[] Hash { get; set; }

    /// <summary>
    /// Cấp độ quyền truy cập của người dùng trong hệ thống.
    /// </summary>
    [Column("Role")]
    [Required]
    public PermissionLevel Role { get; set; } = PermissionLevel.User;

    /// <summary>
    /// Số lần đăng nhập thất bại liên tiếp.
    /// </summary>
    [Required]
    public int FailedLoginCount { get; set; } = 0;

    /// <summary>
    /// Thời điểm lần đăng nhập sai cuối cùng.
    /// </summary>
    public DateTime? LastFailedLoginAt { get; set; }

    /// <summary>
    /// Trạng thái hoạt động của tài khoản.
    /// </summary>
    public bool IsActive { get; set; } = false;

    /// <summary>
    /// Ngày tạo tài khoản.
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    #endregion Properties

    #region Methods

    /// <summary>
    /// Cố gắng phân tích một mảng byte thành đối tượng <see cref="Credentials"/>.
    /// </summary>
    /// <param name="span">Dữ liệu thô chứa payload.</param>
    /// <param name="result">Kết quả phân tích nếu thành công, ngược lại là null.</param>
    /// <returns>True nếu phân tích thành công; false nếu thất bại.</returns>
    public static bool TryParse(
        ReadOnlySpan<byte> span,
        [NotNullWhen(true)] out Credentials result)
    {
        result = null;

        if (span.Length < 2)
            return false;

        byte usernameLength = span[0];
        if (span.Length < 1 + usernameLength + 1)
            return false;

        ReadOnlySpan<byte> usernameSpan = span.Slice(1, usernameLength);
        byte passwordLength = span[1 + usernameLength];

        if (span.Length < 2 + usernameLength + passwordLength)
            return false;

        ReadOnlySpan<byte> passwordSpan = span.Slice(2 + usernameLength, passwordLength);

        result = new Credentials()
        {
            Username = Encoding.UTF8.GetString(usernameSpan),
            Password = Encoding.UTF8.GetString(passwordSpan),
        };
        return true;
    }

    /// <summary>
    /// Tạo mảng byte từ tên người dùng và mật khẩu để gửi đi.
    /// Kiểm tra độ dài hợp lệ của username và password trước khi build.
    /// </summary>
    /// <param name="username">Tên người dùng.</param>
    /// <param name="password">Mật khẩu.</param>
    /// <returns>Mảng byte chứa dữ liệu đã được mã hóa theo định dạng.</returns>
    /// <exception cref="ArgumentException">Ném ra nếu tên người dùng hoặc mật khẩu không hợp lệ hoặc không đúng độ dài quy định.</exception>
    public static byte[] Build(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username is required", nameof(username));
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password is required", nameof(password));

        if (username.Length < UsernameMinLength || username.Length > UsernameMaxLength)
            throw new ArgumentException(
                $"Username must be between {UsernameMinLength} and {UsernameMaxLength} characters.", nameof(username));

        if (password.Length < PasswordMinLength || password.Length > PasswordMaxLength)
            throw new ArgumentException(
                $"Password must be between {PasswordMinLength} and {PasswordMaxLength} characters.", nameof(password));

        byte[] usernameBytes = Encoding.UTF8.GetBytes(username);
        byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

        if (usernameBytes.Length > byte.MaxValue)
            throw new ArgumentException("Username too long in bytes", nameof(username));
        if (passwordBytes.Length > byte.MaxValue)
            throw new ArgumentException("Password too long in bytes", nameof(password));

        byte[] buffer = new byte[1 + usernameBytes.Length + 1 + passwordBytes.Length];

        buffer[0] = (byte)usernameBytes.Length;
        Buffer.BlockCopy(usernameBytes, 0, buffer, 1, usernameBytes.Length);

        buffer[1 + usernameBytes.Length] = (byte)passwordBytes.Length;
        Buffer.BlockCopy(passwordBytes, 0, buffer, 2 + usernameBytes.Length, passwordBytes.Length);

        return buffer;
    }

    #endregion Methods
}