using Nalix.Client.Enums;
using Nalix.Rendering.Attributes;
using Nalix.Rendering.Effects.Parallax;
using Nalix.Rendering.Input;
using Nalix.Rendering.Objects;
using Nalix.Rendering.Runtime;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace Nalix.Client.Objects.Notifications;

/// <summary>
/// Đại diện cho một hộp thông báo trong giao diện người dùng của trò chơi.
/// Hộp thông báo hiển thị một thông điệp văn bản và có thể bao gồm một nút bấm tùy chọn.
/// </summary>
[IgnoredLoad("RenderObject")]
public class NotificationBox : RenderObject
{
    private readonly Text _buttonText;
    private readonly Text _messageText;
    private readonly NineSlicePanel _panel;
    private readonly NineSlicePanel _buttonPanel;

    private Vector2f _textAnchor;
    private System.Single _hoverAnim;
    private System.Boolean _isHovering = false;

    private readonly Color BaseGray = new(220, 220, 220, 255); // light gray (idle)
    private readonly Color HoverGray = new(120, 120, 120, 255); // dark gray (hover)

    /// <summary>
    /// Khởi tạo một hộp thông báo với thông điệp ban đầu và vị trí hiển thị.
    /// </summary>
    /// <param name="initialMessage">Thông điệp ban đầu hiển thị trong hộp thông báo. Mặc định là chuỗi rỗng.</param>
    /// <param name="side">Vị trí hiển thị của hộp thông báo (Top hoặc Bottom). Mặc định là Bottom.</param>
    /// <summary>
    /// Notification box using a 9-slice panel to keep corners/edges crisp when scaled.
    /// </summary>
    public NotificationBox(System.String initialMessage = "", Side side = Side.Bottom)
    {
        // Load assets
        Font font = Assets.Font.Load("1");
        Texture frameTex = Assets.UiTextures.Load("transparent_center/010");
        frameTex.Smooth = false; // pixel-crisp for UI

        // Layout constants
        const System.Single textCharSize = 20f;
        const System.Single horizontalPadding = 12f;  // inner padding in screen pixels
        const System.Single verticalPadding = 10f;  // inner padding top/bottom
        const System.Single verticalGap = 6f;   // gap between text and button

        // Nine-slice border in *source pixels* (match your PNG corners/edges)
        var border = new Thickness(32);

        // Y anchor depending on side
        System.Single floatY = (side == Side.Bottom)
            ? GameEngine.ScreenSize.Y * 0.60f
            : GameEngine.ScreenSize.Y * 0.10f;

        // ---- Create panel with a provisional size (we will resize after text measure)
        _panel = new NineSlicePanel(frameTex, border);

        // Target width: a percentage of screen, clamped
        System.Single targetWidth = System.MathF.Round(System.MathF.Min(GameEngine.ScreenSize.X * 0.85f, 720f));
        System.Single xCentered = System.MathF.Round((GameEngine.ScreenSize.X - targetWidth) / 2f);

        // TEMP size to measure text
        _panel.Position = new SFML.System.Vector2f(xCentered, floatY);
        _panel.Size = new SFML.System.Vector2f(targetWidth, 64f);
        _panel.Layout(); // setup slices

        // ---- Compose text with wrapping based on inner width
        // Inner width = panel width minus left/right borders minus padding
        System.Single innerWidth = targetWidth - (border.Left + border.Right) - (horizontalPadding * 2f);
        if (innerWidth < 50f)
        {
            innerWidth = 50f; // safety
        }

        var wrapped = WrapText(font, initialMessage, (System.UInt32)textCharSize, innerWidth);
        _messageText = new Text(wrapped, font, (System.UInt32)textCharSize)
        {
            FillColor = Color.Black
        };

        // Center origin (accounts for font left/top bearing)
        var lb = _messageText.GetLocalBounds();
        _messageText.Origin = new Vector2f(lb.Left + (lb.Width / 2f), lb.Top + (lb.Height / 2f));

        // Measure text height (using GlobalBounds after origin set)
        var textBounds = _messageText.GetGlobalBounds();
        System.Single textHeight = textBounds.Height;

        // ---- (Optional) create button if Bottom
        System.Single buttonBlockHeight = 0f;
        if (side == Side.Bottom)
        {
            Texture frameBut = Assets.UiTextures.Load("transparent_border/001");
            frameBut.Smooth = false;

            _buttonText = new Text("OK", font, 18) { FillColor = Color.Black };
            var lbBtn = _buttonText.GetLocalBounds();
            _buttonText.Origin = new Vector2f(lbBtn.Left + (lbBtn.Width / 2f), lbBtn.Top + (lbBtn.Height / 2f));

            // Padding inside the button
            const System.Single btnPadY = 6f;
            const System.Single scale = 0.5f; // 80% size

            // Target width: match the inner text area width
            System.Single btnTargetWidth = System.MathF.Round(innerWidth);

            // Target height: fit text height + padding + borders
            System.Single btnTargetHeight = System.MathF.Round(
                lbBtn.Height + (btnPadY * 2f) + border.Top + border.Bottom
            );
            btnTargetHeight = System.MathF.Max(btnTargetHeight, 28f); // minimal cosmetic height

            _buttonPanel = new NineSlicePanel(frameBut, border)
            {
                // Position set later after we know text position
                Position = new Vector2f(0f, 0f),
                Size = new Vector2f(btnTargetWidth * (scale - 0.1f), btnTargetHeight * scale)
            };
            _buttonPanel.Layout();

            // Reserve vertical space under the text
            buttonBlockHeight = verticalGap + _buttonPanel.Size.Y;
        }

        // ---- Compute final panel height to fit text (+ button if any) with padding
        System.Single contentHeight = textHeight + buttonBlockHeight;
        System.Single targetHeight = border.Top + verticalPadding + contentHeight + (verticalPadding + border.Bottom);

        // Minimum height for aesthetics
        targetHeight = System.MathF.Max(targetHeight, 72f);

        // Apply final size and layout
        _panel.Size = new SFML.System.Vector2f(targetWidth, System.MathF.Round(targetHeight));
        _panel.Layout();

        // ---- Compute inner content rect (pixel-perfect)
        System.Single innerLeft = System.MathF.Round(_panel.Position.X + border.Left + horizontalPadding);
        System.Single innerTop = System.MathF.Round(_panel.Position.Y + border.Top + verticalPadding);
        System.Single innerRight = System.MathF.Round(_panel.Position.X + _panel.Size.X - border.Right - horizontalPadding);
        System.Single innerCenterX = System.MathF.Round((innerLeft + innerRight) / 2f);

        // (giữ wrap theo panel như hiện tại)
        _messageText.Position = new Vector2f(innerCenterX, innerTop + (textHeight / 2f));

        // ✅ Lưu neo cố định cho text (tâm text)
        _textAnchor = _messageText.Position;

        // === Position button under text (button độc lập, KHÔNG sửa _messageText)
        if (_buttonPanel != null && _buttonText != null)
        {
            var textGB = _messageText.GetGlobalBounds();
            System.Single buttonY = System.MathF.Round(textGB.Top + textGB.Height + verticalGap);
            buttonY += 30;

            // Bạn muốn button canh giữa theo inner area (không theo text)
            System.Single btnX = System.MathF.Round(innerCenterX - (_buttonPanel.Size.X / 2f));

            _buttonPanel.Position = new Vector2f(btnX, buttonY);
            _buttonPanel.Layout();

            System.Single btnCenterX = System.MathF.Round(btnX + (_buttonPanel.Size.X / 2f));
            System.Single btnCenterY = System.MathF.Round(buttonY + (_buttonPanel.Size.Y / 2f));
            _buttonText.Position = new Vector2f(btnCenterX, btnCenterY);
        }

        Reveal();
        SetZIndex(ZIndex.Notification.ToInt());
    }

    /// <summary>
    /// Cập nhật thông điệp hiển thị trong hộp thông báo.
    /// </summary>
    /// <param name="newMessage">Thông điệp mới cần hiển thị.</param>
    public void UpdateMessage(System.String newMessage)
    {
        const System.Single horizontalPadding = 12f;
        var border = new Thickness(32);

        // wrap theo panel (giữ nguyên hành vi “text trong khung”)
        System.Single innerWidth = _panel.Size.X - (border.Left + border.Right) - (horizontalPadding * 2f);
        if (innerWidth < 50f)
        {
            innerWidth = 50f;
        }

        System.String wrapped = WrapText(_messageText.Font, newMessage, _messageText.CharacterSize, innerWidth);
        _messageText.DisplayedString = wrapped;

        // re-center origin and KEEP fixed position
        var lb = _messageText.GetLocalBounds();
        _messageText.Origin = new Vector2f(lb.Left + (lb.Width / 2f), lb.Top + (lb.Height / 2f));

        // ✅ giữ đúng neo cũ
        _messageText.Position = _textAnchor;

        // nếu có button: chỉ cập nhật lại vị trí button dưới text (không đụng text)
        const System.Single verticalGap = 20f;
        if (_buttonPanel != null && _buttonText != null)
        {
            var textGB = _messageText.GetGlobalBounds();
            System.Single buttonY = System.MathF.Round(textGB.Top + textGB.Height + verticalGap);

            // canh giữa theo inner area
            System.Single innerLeft = System.MathF.Round(_panel.Position.X + border.Left + horizontalPadding);
            System.Single innerRight = System.MathF.Round(_panel.Position.X + _panel.Size.X - border.Right - horizontalPadding);
            System.Single innerCenterX = System.MathF.Round((innerLeft + innerRight) / 2f);

            System.Single btnX = System.MathF.Round(innerCenterX - (_buttonPanel.Size.X / 2f));

            _buttonPanel.Position = new Vector2f(btnX, buttonY);
            _buttonPanel.Layout();

            System.Single btnCenterX = System.MathF.Round(btnX + (_buttonPanel.Size.X / 2f));
            System.Single btnCenterY = System.MathF.Round(buttonY + (_buttonPanel.Size.Y / 2f));
            _buttonText.Position = new Vector2f(btnCenterX, btnCenterY);
        }
    }

    /// <summary>
    /// Cập nhật trạng thái của hộp thông báo, xử lý tương tác chuột (hover, click).
    /// </summary>
    /// <param name="deltaTime">Thời gian trôi qua kể từ lần cập nhật trước (tính bằng giây).</param>
    public override void Update(System.Single deltaTime)
    {
        if (!Visible)
        {
            return;
        }

        if (_buttonPanel == null)
        {
            return; // no button in Top mode
        }

        Vector2i mousePos = InputState.GetMousePosition();

        // Hit test on the 9-slice button panel
        var hoverRect = new FloatRect(
            _buttonPanel.Position.X,
            _buttonPanel.Position.Y,
            _buttonPanel.Size.X,
            _buttonPanel.Size.Y
        );

        System.Boolean hover = hoverRect.Contains(mousePos.X, mousePos.Y);

        const System.Single fadeIn = 0.08f, fadeOut = 0.10f;

        if (hover)
        {
            _isHovering = true;
            _hoverAnim += deltaTime / fadeIn;
        }
        else
        {
            _isHovering = false;
            _hoverAnim -= deltaTime / fadeOut;
        }

        _hoverAnim = System.Math.Clamp(_hoverAnim, 0f, 1f);

        // Visual feedback
        Color btnColor = Lerp(BaseGray, HoverGray, _hoverAnim);
        _buttonPanel.SetColor(btnColor); // requires NineSlicePanel.SetColor(Color)
        if (_buttonText != null)
        {
            _buttonText.FillColor = Lerp(Color.Black, Color.White, _hoverAnim);
        }

        if (_isHovering && InputState.IsMouseButtonPressed(Mouse.Button.Left))
        {
            Conceal();
        }
    }

    /// <summary>
    /// Vẽ hộp thông báo lên mục tiêu render.
    /// </summary>
    /// <param name="target">Mục tiêu render (thường là cửa sổ trò chơi).</param>
    public override void Render(RenderTarget target)
    {
        if (!Visible)
        {
            return;
        }

        target.Draw(_panel);
        target.Draw(_messageText);

        if (_buttonPanel != null)
        {
            target.Draw(_buttonPanel);
            if (_buttonText != null)
            {
                target.Draw(_buttonText);
            }
        }
    }

    /// <summary>
    /// Lấy đối tượng Drawable (không được hỗ trợ, sử dụng Render() thay thế).
    /// </summary>
    /// <returns>Không trả về giá trị, luôn ném ngoại lệ.</returns>
    /// <exception cref="System.NotSupportedException">Ném ra khi phương thức này được gọi.</exception>
    protected override Drawable GetDrawable()
        => throw new System.NotSupportedException("Use Render() instead.");

    /// <summary>
    /// Bọc văn bản để đảm bảo nó vừa với chiều rộng tối đa được chỉ định.
    /// </summary>
    /// <param name="font">Font chữ sử dụng cho văn bản.</param>
    /// <param name="text">Chuỗi văn bản cần bọc.</param>
    /// <param name="characterSize">Kích thước ký tự của văn bản.</param>
    /// <param name="maxWidth">Chiều rộng tối đa của văn bản.</param>
    /// <returns>Chuỗi văn bản đã được bọc dòng.</returns>
    private static System.String WrapText(Font font, System.String text, System.UInt32 characterSize, System.Single maxWidth)
    {
        System.String result = "";
        System.String currentLine = "";
        System.String[] words = text.Split(' ');

        foreach (var word in words)
        {
            System.String testLine = currentLine.Length > 0 ? currentLine + " " + word : word;

            Text tempText = new(testLine, font, characterSize);
            if (tempText.GetLocalBounds().Width > maxWidth)
            {
                result += currentLine + "\n";
                currentLine = word;
            }
            else
            {
                currentLine = testLine;
            }
        }

        result += currentLine;
        return result;
    }

    private static Color Lerp(Color a, Color b, System.Single t)
    {
        System.Byte LerpB(System.Byte x, System.Byte y) => (System.Byte)(x + ((y - x) * t));
        return new Color(
            LerpB(a.R, b.R),
            LerpB(a.G, b.G),
            LerpB(a.B, b.B),
            LerpB(a.A, b.A)
        );
    }
}