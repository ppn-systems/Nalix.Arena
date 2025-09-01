using SFML.Graphics;
using SFML.System;
using System;

namespace Nalix.Rendering.Effects.Transitions.Effects;

/// <summary>Slide cover: một tấm phủ trượt vào giữa rồi trượt ra.</summary>
internal sealed class SlideCoverOverlay : ScreenOverlayBase
{
    private readonly RectangleShape _rect;
    private readonly Boolean _fromLeft;

    public SlideCoverOverlay(Color color, Boolean fromLeft) : base(color)
    {
        _fromLeft = fromLeft;
        _rect = new RectangleShape(Size)
        {
            FillColor = new Color(color.R, color.G, color.B, 255)
        };
    }

    public override void Update(Single p, Boolean closing)
    {
        // closing: đi từ mép vào giữa, opening: đi từ giữa về mép
        Single travel = Size.X; // quãng đường
        Single t = closing ? p : 1f - p;
        Single x = _fromLeft ? -travel + (travel * t) : Size.X + (-travel * t);
        _rect.Position = new Vector2f(x, 0f);
    }

    public override Drawable GetDrawable() => _rect;
}
