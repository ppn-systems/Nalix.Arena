using SFML.Graphics;
using SFML.System;

namespace Nalix.Rendering.Effects.Visual.UI;

/// <summary>
/// Single-line password input built on top of <see cref="InputField"/>.
/// </summary>
/// <remarks>
/// - Masks user input with <see cref="MaskChar"/> by default.<br/>
/// - Set <see cref="Show"/> = <c>true</c> to reveal raw text (useful for an "eye" toggle).<br/>
/// - Constructor sets <see cref="InputField.PasswordMode"/> = <c>true</c>.
/// </remarks>
public sealed class PasswordField : InputField
{
    /// <summary>
    /// Whether to reveal the raw text (i.e., “show password”). Default: <c>false</c>.
    /// </summary>
    public System.Boolean Show { get; set; } = false;

    /// <summary>
    /// Mask character used when <see cref="Show"/> is <c>false</c>. Default: • (U+2022).
    /// </summary>
    public System.Char MaskChar { get; set; } = '\u2022';

    /// <summary>
    /// Creates a new password field.
    /// </summary>
    public PasswordField(
        Texture panelTexture,
        Thickness border,
        IntRect sourceRect,
        Font font,
        System.UInt32 fontSize,
        Vector2f size,
        Vector2f position)
        : base(panelTexture, border, sourceRect, font, fontSize, size, position) =>
        // (VN) Mặc định dùng chế độ password của InputField
        PasswordMode = true;

    /// <summary>
    /// Toggle <see cref="Show"/> state. (VN) Đổi trạng thái hiện/ẩn mật khẩu.
    /// </summary>
    public void Toggle() => Show = !Show;

    /// <summary>
    /// Returns what should be displayed: raw text when <see cref="Show"/> is true,
    /// otherwise masked with <see cref="MaskChar"/>.
    /// </summary>
    protected override System.String GetDisplayText()
    {
        // Nếu đang “show”, hiển thị text thường
        if (Show)
        {
            return Text;
        }

        // Khi ẩn, trả về chuỗi mask có độ dài bằng số ký tự thực
        var len = Text?.Length ?? 0;
        return len == 0 ? System.String.Empty : new System.String(MaskChar, len);
    }
}
