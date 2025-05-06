using Nalix.Game.Client.Desktop.Content;
using Nalix.Game.Client.Desktop.Graphics;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;

namespace Nalix.Game.Client.Desktop.UI;

internal class Button
{
    private static readonly Texture ButtonSheet = new(string.Format(TexturePath.UI, 2));
    private static readonly ImageCutter Cutter = new(ButtonSheet, 96, 32);

    private readonly Sprite _sprite; // Sprite to represent the button
    private readonly Text _text; // Text of the button
    private readonly IntRect _normal; // Normal texture of the button
    private readonly IntRect _pressed; // Pressed texture of the button

    public FloatRect Bounds => _sprite.GetGlobalBounds(); // Get button bounds
    public Action OnClick { get; set; } // Action when button is clicked

    // Button state
    private bool _isPressed;

    public Button(string label, Font font, Vector2f position, Vector2f size)
    {
        //  +--------+--------+
        //  | (0, 0) | (1, 0) |   ← row 0
        //  + -------+--------+
        //  | (0, 1) | (1, 1) |   ← row 1
        //  + -------+--------+
        _normal = Cutter.GetRectAt(0, 0);
        _pressed = Cutter.GetRectAt(1, 0);

        // Create sprite for the button with normal texture
        _sprite = new Sprite(ButtonSheet)
        {
            Position = position,
            TextureRect = _normal,  // Set the cropped rectangle
                                    // Update the sprite size to match the new button size
            Scale = new Vector2f(size.X / _normal.Width, size.Y / _normal.Height)
        };

        // Create text for the button
        _text = new Text(label, font, 24)
        {
            FillColor = Color.White,
            // Calculate appropriate font size based on button size
            // Font size is 1/4th of the smallest button dimension
            CharacterSize = (uint)Math.Min(size.X, size.Y) / 2
        };

        // Center the text inside the button (adjust for texture bounds)
        FloatRect textBounds = _text.GetLocalBounds();

        // Adjust position to make sure text is centered properly
        _text.Position = new Vector2f(
            position.X + (size.X - textBounds.Width) / 2 - textBounds.Left, // Add small offset if needed
            position.Y + (size.Y - textBounds.Height) / 2 - textBounds.Top // Adjust vertical positioning
        );
    }

    // Draw the button on the window
    public void Draw(RenderWindow window)
    {
        window.Draw(_sprite); // Draw the button background (sprite)
        window.Draw(_text); // Draw the text on the button
    }

    // Handle mouse click event
    public void HandleMouseClick(MouseButtonEventArgs mouseButtonEvent)
    {
        if (Bounds.Contains(mouseButtonEvent.X, mouseButtonEvent.Y))
        {
            OnClick?.Invoke(); // Perform the assigned action on click
            _isPressed = !_isPressed; // Toggle the button's pressed state
            UpdateTexture(); // Update the button texture based on the pressed state
        }
    }

    // Handle mouse hover event
    public void HandleMouseHover(Vector2i mousePosition)
    {
        if (Bounds.Contains(mousePosition.X, mousePosition.Y))
            _sprite.Color = new Color(255, 255, 255, 150); // Hover effect (semi-transparent)
        else
            _sprite.Color = new Color(255, 255, 255, 255); // Default color (fully opaque)
    }

    // Update the texture of the button based on its pressed state
    private void UpdateTexture()
    {
        if (_isPressed)
        {
            _sprite.TextureRect = _pressed;
        }
        else
        {
            _sprite.TextureRect = _normal;
        }
    }
}