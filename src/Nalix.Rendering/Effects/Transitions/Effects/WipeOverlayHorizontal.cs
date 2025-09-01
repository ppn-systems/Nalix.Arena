using SFML.Graphics;
using SFML.System;
using System;

namespace Nalix.Rendering.Effects.Transitions.Effects;

/// <summary>Wipe ngang: che từ trái→phải, rồi mở ngược lại.</summary>
internal sealed class WipeOverlayHorizontal : ScreenOverlayBase
{
    private readonly RectangleShape _rect;

    public WipeOverlayHorizontal(Color color) : base(color)
    {
        _rect = new RectangleShape(new Vector2f(0, Size.Y))
        {
            FillColor = new Color(color.R, color.G, color.B, 255),
            Position = new Vector2f(0, 0)
        };
    }

    public override void Update(Single p, Boolean closing)
    {
        Single w = closing ? Size.X * p : Size.X * (1f - p);
        _rect.Size = new Vector2f(Math.Max(0f, w), Size.Y);
    }

    public override Drawable GetDrawable() => _rect;
}
