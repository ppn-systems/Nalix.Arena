using SFML.Graphics;
using SFML.System;
using System;

namespace Nalix.Rendering.Effects.Transitions.Effects;

/// <summary>Wipe dọc: che từ trên→dưới, rồi mở ngược lại.</summary>
internal sealed class WipeOverlayVertical : ScreenOverlayBase
{
    private readonly RectangleShape _rect;

    public WipeOverlayVertical(Color color) : base(color)
    {
        _rect = new RectangleShape(new Vector2f(Size.X, 0))
        {
            FillColor = new Color(color.R, color.G, color.B, 255),
            Position = new Vector2f(0, 0)
        };
    }

    public override void Update(Single p, Boolean closing)
    {
        Single h = closing ? Size.Y * p : Size.Y * (1f - p);
        _rect.Size = new Vector2f(Size.X, Math.Max(0f, h));
    }

    public override Drawable GetDrawable() => _rect;
}