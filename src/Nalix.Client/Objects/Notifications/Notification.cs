using Nalix.Client.Enums;
using Nalix.Rendering.Attributes;
using Nalix.Rendering.Effects.Visual;
using Nalix.Rendering.Objects;
using Nalix.Rendering.Runtime;
using SFML.Graphics;
using SFML.System;
using System;

namespace Nalix.Client.Objects.Notifications;

/// <summary>
/// Lightweight notification box without button. Renders a nine-slice panel with wrapped text.
/// </summary>
[IgnoredLoad("RenderObject")]
public class Notification : RenderObject
{
    protected readonly Text _messageText;
    protected readonly NineSlicePanel _panel;

    protected Vector2f _textAnchor;
    protected readonly Thickness _border = new(32);

    // Layout constants shared with derived classes
    protected const Single TextCharSize = 20f;
    protected const Single HorizontalPadding = 12f;
    protected const Single VerticalPadding = 30f;

    /// <summary>
    /// Initializes a notification box with text only.
    /// </summary>
    /// <param name="initialMessage">Initial message.</param>
    /// <param name="side">Top/Bottom placement.</param>
    public Notification(String initialMessage = "", Side side = Side.Top)
    {
        // Load assets
        Font font = Assets.Font.Load("1");
        Texture frameTex = Assets.UiTextures.Load("transparent_center/010");
        frameTex.Smooth = false;

        // Compute basic layout
        Single floatY = side == Side.Bottom ? GameEngine.ScreenSize.Y * 0.60f : GameEngine.ScreenSize.Y * 0.10f;
        Single targetWidth = MathF.Round(MathF.Min(GameEngine.ScreenSize.X * 0.85f, 720f));
        Single xCentered = MathF.Round((GameEngine.ScreenSize.X - targetWidth) / 2f);

        // Create panel (size will be finalized after measuring text)
        _panel = new NineSlicePanel(frameTex, _border)
        {
            Position = new Vector2f(xCentered, floatY),
            Size = new Vector2f(targetWidth, 64f)
        };
        _panel.Layout();

        // Prepare wrapped text
        Single innerWidth = targetWidth - (_border.Left + _border.Right) - (HorizontalPadding * 2f);
        if (innerWidth < 50f)
        {
            innerWidth = 50f;
        }

        String wrapped = WrapText(font, initialMessage, (UInt32)TextCharSize, innerWidth);
        _messageText = new Text(wrapped, font, (UInt32)TextCharSize) { FillColor = Color.Black };

        // Center origin and measure
        var lb = _messageText.GetLocalBounds();
        _messageText.Origin = new Vector2f(lb.Left + (lb.Width / 2f), lb.Top + (lb.Height / 2f));
        Single textHeight = _messageText.GetGlobalBounds().Height;

        // Final panel height
        Single targetHeight = _border.Top + VerticalPadding + textHeight + VerticalPadding + _border.Bottom;
        targetHeight = MathF.Max(targetHeight, 72f);

        _panel.Size = new Vector2f(targetWidth, MathF.Round(targetHeight));
        _panel.Layout();

        // Inner rect and text position
        Single innerLeft = MathF.Round(_panel.Position.X + _border.Left + HorizontalPadding);
        Single innerRight = MathF.Round(_panel.Position.X + _panel.Size.X - _border.Right - HorizontalPadding);
        Single innerCenterX = MathF.Round((innerLeft + innerRight) / 2f);
        Single innerTop = MathF.Round(_panel.Position.Y + _border.Top + VerticalPadding);

        _messageText.Position = new Vector2f(innerCenterX, innerTop + (textHeight / 2f));
        _textAnchor = _messageText.Position;

        Reveal();
        SetZIndex(ZIndex.Notification.ToInt());
    }

    /// <summary>
    /// Updates the message keeping the same anchor point.
    /// </summary>
    /// <param name="newMessage">New text.</param>
    public virtual void UpdateMessage(String newMessage)
    {
        Single innerWidth = _panel.Size.X - (_border.Left + _border.Right) - (HorizontalPadding * 2f);
        if (innerWidth < 50f)
        {
            innerWidth = 50f;
        }

        String wrapped = WrapText(_messageText.Font, newMessage, _messageText.CharacterSize, innerWidth);
        _messageText.DisplayedString = wrapped;

        var lb = _messageText.GetLocalBounds();
        _messageText.Origin = new Vector2f(lb.Left + (lb.Width / 2f), lb.Top + (lb.Height / 2f));

        // Keep fixed anchor
        _messageText.Position = _textAnchor;
    }

    /// <inheritdoc />
    public override void Update(Single deltaTime)
    {
        if (!Visible)
        {
            return;
        }
        // No-op for the base (text-only) box
    }

    /// <inheritdoc />
    public override void Render(RenderTarget target)
    {
        if (!Visible)
        {
            return;
        }

        target.Draw(_panel);
        target.Draw(_messageText);
    }

    /// <inheritdoc />
    protected override Drawable GetDrawable()
        => throw new System.NotSupportedException("Use Render() instead.");

    /// <summary>
    /// Word-wrap helper that fits text into a max width.
    /// </summary>
    protected static String WrapText(Font font, String text, UInt32 characterSize, Single maxWidth)
    {
        String result = "";
        String currentLine = "";
        String[] words = text.Split(' ');

        foreach (var word in words)
        {
            String testLine = currentLine.Length > 0 ? currentLine + " " + word : word;
            Text temp = new(testLine, font, characterSize);
            if (temp.GetLocalBounds().Width > maxWidth)
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

    /// <summary>
    /// Color lerp helper.
    /// </summary>
    protected static Color Lerp(Color a, Color b, Single t)
    {
        Byte L(Byte x, Byte y) => (Byte)(x + ((y - x) * t));
        return new Color(L(a.R, b.R), L(a.G, b.G), L(a.B, b.B), L(a.A, b.A));
    }
}

