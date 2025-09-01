using SFML.Graphics;
using System;

namespace Nalix.Rendering.Effects.Transitions.Effects;

/// <summary>Fade: chỉnh alpha từ 0→255 rồi 255→0.</summary>
internal sealed class FadeOverlay : ScreenOverlayBase
{
    private readonly RectangleShape _rect;

    public FadeOverlay(Color color) : base(color)
        => _rect = new RectangleShape(Size) { FillColor = new Color(color.R, color.G, color.B, 0) };

    public override void Update(Single p, Boolean closing)
    {
        // closing: alpha tăng 0→255, opening: alpha giảm 255→0
        Byte a = closing ? (Byte)(255 * p) : (Byte)(255 * (1f - p));
        _rect.FillColor = new Color(BaseColor.R, BaseColor.G, BaseColor.B, a);
    }

    public override Drawable GetDrawable() => _rect;
}
