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

    public NotificationBox(string initialMessage = "")
    {
        base.SetZIndex(ZIndex.Notification.ToInt());
        base.Reveal();

        Font font = Assets.Font.Load("1.ttf");
        Texture bgTexture = Assets.UI.Load("dialog/7.png");
        Texture acceptButtonTexture = Assets.UI.Load("buttons/3.png");

        // Background
        _background = new Sprite(bgTexture)
        {
            Position = new Vector2f(
                (GameEngine.ScreenSize.X - bgTexture.Size.X) / 2f, // canh giữa ngang
                GameEngine.ScreenSize.Y - bgTexture.Size.Y - 70     // cách đáy 20px
            )
        };

        Vector2f backgroundSize = new(bgTexture.Size.X, bgTexture.Size.Y);

        // Text
        float maxTextWidth = bgTexture.Size.X - 40; // padding 20 trái + phải

        _messageText = new Text(
            WrapText(font, initialMessage, 20, maxTextWidth),
            font, 20)
        {
            FillColor = Color.Black,
            Position = _background.Position + new Vector2f(55, 45),
        };

        // Accept Button
        _acceptButtonSprite = new Sprite(acceptButtonTexture)
        {
            Position = _background.Position + new Vector2f(
                backgroundSize.X - acceptButtonTexture.Size.X - 20,
                backgroundSize.Y - acceptButtonTexture.Size.Y - 20),
        };
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

    private static string WrapText(Font font, string text, uint characterSize, float maxWidth)
    {
        string[] words = text.Split(' ');
        string result = "";
        string currentLine = "";

        foreach (var word in words)
        {
            string testLine = currentLine.Length > 0 ? currentLine + " " + word : word;

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
}