using Nalix.Game.Presentation.Enums;
using Nalix.Graphics;
using Nalix.Graphics.Rendering.Object;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;

namespace Nalix.Game.Presentation.Objects;

public class NotificationBox : RenderObject
{
    private Action _onAcceptClicked;
    private readonly Text _messageText;
    private readonly Sprite _acceptButtonSprite;
    private readonly Sprite _background;

    public NotificationBox(
        Vector2f position,
        Vector2f size,
        string initialMessage = "",
        Sprite acceptButtonSprite = null)
    {
        base.SetZIndex(ZIndex.Notification.ToInt()); // ưu tiên hiển thị trên cùng
        base.Reveal();

        Font font = Assets.Font.Load("1.ttf");
        Texture bg = Assets.UI.Load("dialog/7.png");

        // Background
        _background = new Sprite(bg)
        {
            Position = new Vector2f(position.X + size.X - bg.Size.X - 20, position.Y + size.Y - bg.Size.Y - 20),
        };

        // Text thông báo
        _messageText = new Text(initialMessage, font, 20)
        {
            FillColor = Color.White,
            Position = new Vector2f(position.X + 20, position.Y + 20),
            // Giới hạn chiều rộng (có thể tự xử lý wrap nếu cần)
        };

        // Nút đồng ý
        if (acceptButtonSprite != null)
        {
            _acceptButtonSprite = new Sprite(acceptButtonSprite.Texture)
            {
                Position = new Vector2f(position.X + size.X - acceptButtonSprite.Texture.Size.X - 20, position.Y + size.Y - acceptButtonSprite.Texture.Size.Y - 20),
                Scale = acceptButtonSprite.Scale,
            };
        }
        else
        {
            // Nếu không có sprite, tạo nút hình chữ nhật đơn giản
            _acceptButtonSprite = new Sprite
            {
                Texture = CreateSolidTexture((uint)100, (uint)40, new Color(0, 120, 0)),
                Position = new Vector2f(position.X + size.X - 120, position.Y + size.Y - 60)
            };
        }
    }

    /// <summary>
    /// Hiển thị thông báo mới với nội dung và callback khi nút đồng ý được nhấn.
    /// </summary>
    public void Show(string message, Action onAcceptClicked)
    {
        _messageText.DisplayedString = message;
        _onAcceptClicked = onAcceptClicked;
        base.Reveal();
    }

    public override void Update(float deltaTime)
    {
        if (!Visible) return;

        // Kiểm tra input chuột cho nút đồng ý
        if (InputState.IsMouseButtonPressed(Mouse.Button.Left))
        {
            Vector2i mousePos = InputState.GetMousePosition();
            if (_acceptButtonSprite.GetGlobalBounds().Contains(mousePos.X, mousePos.Y))
            {
                _onAcceptClicked?.Invoke();
                base.Conceal();
            }
        }
    }

    public override void Render(RenderTarget target)
    {
        if (!Visible) return;

        target.Draw(_background);
        target.Draw(_messageText);
        target.Draw(_acceptButtonSprite);
    }

    protected override Drawable GetDrawable()
        => throw new NotSupportedException("Use Render() instead.");

    // Helper tạo texture đơn sắc cho nút nếu bạn không dùng sprite
    private static Texture CreateSolidTexture(uint width, uint height, Color color)
    {
        Image img = new(width, height, color);
        return new Texture(img);
    }
}