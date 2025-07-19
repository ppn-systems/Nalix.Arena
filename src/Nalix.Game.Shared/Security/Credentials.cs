﻿using Nalix.Common.Security.Types;
using Nalix.Common.Serialization;
using Nalix.Common.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

    public const System.Int32 UsernameMinLength = 3;
    public const System.Int32 UsernameMaxLength = 20;

    public const System.Int32 PasswordMinLength = 8;
    public const System.Int32 PasswordMaxLength = 50;

    #endregion Constants

    #region Properties

    /// <summary>
    /// ID của tài khoản trong cơ sở dữ liệu.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public System.Int32 Id { get; private set; }

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
    public System.String Username { get; set; }

    /// <summary>
    /// Mật khẩu (chỉ dùng trong code, không lưu vào database).
    /// </summary>
    [NotMapped]
    [SerializeOrder(1)]
    public System.String Password { get; set; }

    /// <summary>
    /// Chuỗi salt ngẫu nhiên được tạo ra để băm mật khẩu.
    /// Salt giúp bảo vệ mật khẩu khỏi các cuộc tấn công từ điển và rainbow table.
    /// </summary>
    [Required]
    [MaxLength(64)]
    [Column(TypeName = "binary(64)")]
    public System.Byte[] Salt { get; set; }

    /// <summary>
    /// Mật khẩu sau khi được băm bằng thuật toán PBKDF2.
    /// Giá trị này được lưu trữ trong cơ sở dữ liệu để xác minh mật khẩu khi đăng nhập.
    /// </summary>
    [Required]
    [MaxLength(64)]
    [Column(TypeName = "binary(64)")]
    public System.Byte[] Hash { get; set; }

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
    public System.Int32 FailedLoginCount { get; set; } = 0;

    /// <summary>
    /// Thời điểm lần đăng nhập sai cuối cùng.
    /// </summary>
    public System.DateTime? LastFailedLoginAt { get; set; }

    /// <summary>
    /// Trạng thái hoạt động của tài khoản.
    /// </summary>
    public System.Boolean IsActive { get; set; } = false;

    /// <summary>
    /// Ngày tạo tài khoản.
    /// </summary>
    [Required]
    public System.DateTime CreatedAt { get; set; } = System.DateTime.UtcNow;

    #endregion Properties
}