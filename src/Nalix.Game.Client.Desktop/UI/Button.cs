using SFML.Graphics;
using SFML.System;
using System;

namespace Nalix.Game.Client.Desktop.UI;

internal class Button
{
    private readonly RectangleShape _shape;
    private readonly Text _text;

    public FloatRect Bounds => _shape.GetGlobalBounds();
    public Action OnClick { get; set; }

    public Button(string label, Font font, Vector2f position, Vector2f size)
    {
        _shape = new RectangleShape(size)
        {
            Position = position,
            FillColor = new Color(70, 70, 70),
            OutlineColor = Color.White,
            OutlineThickness = 2
        };

        FloatRect bounds = _text.GetLocalBounds();

        _text = new Text(label, font, 24)
        {
            FillColor = Color.White,
            Position = new Vector2f(
                position.X + (size.X - bounds.Width) / 2 - bounds.Left,
                position.Y + (size.Y - bounds.Height) / 2 - bounds.Top
            )
        };
    }

    public void Draw(RenderWindow window)
    {
        window.Draw(_shape);
        window.Draw(_text);
    }

    public void HandleMouseClick(Vector2i mousePosition)
    {
        if (Bounds.Contains(mousePosition.X, mousePosition.Y))
            OnClick?.Invoke();
    }

    public void HandleMouseHover(Vector2i mousePosition)
    {
        if (Bounds.Contains(mousePosition.X, mousePosition.Y))
            _shape.FillColor = new Color(90, 90, 90); // hover
        else
            _shape.FillColor = new Color(70, 70, 70); // default
    }
}