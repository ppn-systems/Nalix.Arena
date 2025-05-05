using SFML.Graphics;
using SFML.System;

namespace Nalix.Game.Client.Desktop.UI;

internal class Label
{
    private readonly Text _text;

    public Label(string content, Font font, uint size, Vector2f position, Color? color = null)
    {
        _text = new Text(content, font, size)
        {
            FillColor = color ?? Color.White
        };

        // Căn giữa text so với vị trí
        FloatRect bounds = _text.GetLocalBounds();
        _text.Origin = new Vector2f(bounds.Left + bounds.Width / 2f, bounds.Top + bounds.Height / 2f);
        _text.Position = position;
    }

    public void SetText(string content)
    {
        _text.DisplayedString = content;

        // Cập nhật lại Origin sau khi đổi nội dung
        FloatRect bounds = _text.GetLocalBounds();
        _text.Origin = new Vector2f(bounds.Left + bounds.Width / 2f, bounds.Top + bounds.Height / 2f);
    }

    public void Draw(RenderWindow window)
    {
        window.Draw(_text);
    }

    public Vector2f Position
    {
        get => _text.Position;
        set => _text.Position = value;
    }

    public string Text
    {
        get => _text.DisplayedString;
        set => SetText(value);
    }

    public Color FillColor
    {
        get => _text.FillColor;
        set => _text.FillColor = value;
    }
}