using SFML.Graphics;
using SFML.System;
using System;

namespace Nalix.Rendering.Effects.Transitions.Effects;

/// <summary>Zoom: hình chữ nhật scale từ tâm (ZoomIn: 0→1 rồi 1→0; ZoomOut: 1→0 rồi 0→1).</summary>
internal sealed class ZoomOverlay : ScreenOverlayBase
{
    private readonly RectangleShape _rect;
    private readonly Boolean _modeIn; // true: ZoomIn, false: ZoomOut

    public ZoomOverlay(Color color, Boolean modeIn) : base(color)
    {
        _modeIn = modeIn;
        _rect = new RectangleShape(Size)
        {
            Origin = Size / 2f,
            Position = Size / 2f,
            FillColor = new Color(color.R, color.G, color.B, 255)
        };
    }

    public override void Update(Single p, Boolean closing)
    {
        // Với ZoomIn: closing scale 0→1; opening 1→0
        // Với ZoomOut: closing 1→0; opening 0→1
        Single s = _modeIn ? closing ? p : 1f - p : closing ? 1f - p : p;
        s = Math.Clamp(s, 0.0001f, 1f);
        _rect.Scale = new Vector2f(s, s);
    }

    public override Drawable GetDrawable() => _rect;
}
