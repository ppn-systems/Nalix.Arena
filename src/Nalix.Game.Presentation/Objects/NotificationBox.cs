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
    private readonly bool _hasButton;
    private readonly Text _messageText;
    private readonly Sprite _background;

    private readonly Sprite _button;
    private readonly Text _buttonText;

    private float _hoverTime = 0f;
    private bool _isHovering = false;

    public NotificationBox(string initialMessage = "", bool button = true, Side side = Side.Bottom)
    {
        _hasButton = button;

        Font font = Assets.Font.Load("1.ttf");
        Texture bgTexture = Assets.UI.Load("dialog/7.png");

        float floatY;

        // Background
        if (side == Side.Bottom)
        {
            floatY = GameEngine.ScreenSize.Y * 0.6f;
        }
        else
        {
            // If the notification is at the top, position it accordingly
            floatY = GameEngine.ScreenSize.Y * 0.1f;
        }

        _background = new Sprite(bgTexture)
        {
            Scale = new Vector2f(0.8f, 0.8f),
            Position = new Vector2f((GameEngine.ScreenSize.X - (0.8f * bgTexture.Size.X)) / 2f, floatY)
        };

        // Text
        _messageText = new Text(WrapText(font, initialMessage, 20, bgTexture.Size.X * 0.7f), font, 20)
        {
            FillColor = Color.Black,
        };

        // Lấy thông tin kích thước của text để căn giữa
        FloatRect textBounds = _messageText.GetLocalBounds();

        // Đặt gốc về giữa text (cho phép Position là tâm)
        _messageText.Origin = new Vector2f(
            textBounds.Left + (textBounds.Width / 2f),
            textBounds.Top + (textBounds.Height / 2f)
        );

        // Đặt vị trí text vào chính giữa background
        _messageText.Position = new Vector2f(
            _background.Position.X + (_background.GetGlobalBounds().Width / 2f),
            _background.Position.Y + (_background.GetGlobalBounds().Height / 2f)
        );

        if (_hasButton)
        {
            _button = new Sprite(Assets.UI.Load("button/7.png"))
            {
                Scale = new Vector2f(0.5f, 0.5f)
            };

            float buttonY = _messageText.Position.Y + _messageText.GetLocalBounds().Height + 5f;

            // Center button horizontally below the message text
            _button.Position = new Vector2f(
                _background.Position.X +
                (_background.GetGlobalBounds().Width / 2f) -
                (_button.GetGlobalBounds().Width / 2f),
                buttonY
            );

            _buttonText = new Text("Ok", font, 18)
            {
                FillColor = Color.White
            };

            this.CenterTextOnSprite();
        }

        base.Reveal();
        base.SetZIndex(ZIndex.Notification.ToInt());
    }

    public override void Update(float deltaTime)
    {
        if (!Visible) return;
        Vector2i mousePos = InputState.GetMousePosition();
        bool currentlyHovering = _button.GetGlobalBounds().Contains(mousePos.X, mousePos.Y);

        if (currentlyHovering)
        {
            _hoverTime += deltaTime;

            if (_hoverTime >= 0.1f)
            {
                if (!_isHovering)
                {
                    _isHovering = true;
                    _button.Texture = Assets.UI.Load("button/8.png"); // Change to hover texture
                }
            }
        }
        else
        {
            if (_isHovering)
            {
                _isHovering = false;
                _button.Texture = Assets.UI.Load("button/7.png"); // Change back to normal texture
            }
            _hoverTime = 0f;
        }

        // Check click
        if (InputState.IsMouseButtonPressed(Mouse.Button.Left) && _isHovering)
        {
            base.Conceal();
        }
    }

    public override void Render(RenderTarget target)
    {
        if (!Visible) return;

        target.Draw(_background);
        target.Draw(_messageText);

        if (_hasButton)
        {
            target.Draw(_button);
            target.Draw(_buttonText);
        }
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

    private void CenterTextOnSprite()
    {
        FloatRect textBounds = _buttonText.GetLocalBounds();
        FloatRect spriteBounds = _button.GetGlobalBounds();

        // Set the origin of the text to its center
        _buttonText.Origin = new Vector2f(
            textBounds.Left + (textBounds.Width / 2f),
            textBounds.Top + (textBounds.Height / 2f)
        );

        // Position the text at the center of the sprite
        _buttonText.Position = new Vector2f(
            spriteBounds.Left + (spriteBounds.Width / 2f),
            spriteBounds.Top + (spriteBounds.Height / 2f)
        );
    }
}