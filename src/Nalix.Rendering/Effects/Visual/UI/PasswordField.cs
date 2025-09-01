using SFML.Graphics;
using SFML.System;

namespace Nalix.Rendering.Effects.Visual.UI;

public sealed class PasswordField(
    Texture panelTexture,
    Thickness border,
    IntRect sourceRect,
    Font font,
    System.UInt32 fontSize,
    Vector2f size, Vector2f position) : TextField(panelTexture, border, sourceRect, font, fontSize, size, position)
{
    public System.Boolean Show { get; set; } = false;

    protected override System.String GetDisplayText() => Show ? _buffer.ToString() : new System.String('•', _buffer.Length);
}
