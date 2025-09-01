using SFML.Graphics;
using SFML.System;

namespace Nalix.Rendering.Effects.Visual;

/// <summary>
/// Simple 9-slice panel for crisp, scalable UI frames.
/// Corners unscaled, edges scale in one axis, center in both.
/// </summary>
public sealed class NineSlicePanel : Drawable
{
    // Public readable state
    public Vector2f Position { get; private set; }
    public Vector2f Size { get; private set; }          // Target outer size in pixels
    public IntRect SourceRect { get; private set; }     // Sub-rect inside the texture
    public Thickness Border { get; private set; }       // Border thickness in source pixels
    public Texture Texture { get; }

    private readonly Sprite[] _parts = new Sprite[9];
    private System.Boolean _dirty = true;

    /// <param name="texture">UI frame texture. Set Smooth=false at load time if you need pixel-crisp.</param>
    /// <param name="border">Left/Top/Right/Bottom border thickness (in source pixels)</param>
    /// <param name="sourceRect">Optional source rect; pass default for full texture</param>
    public NineSlicePanel(Texture texture, Thickness border, IntRect sourceRect = default)
    {
        Texture = texture ?? throw new System.ArgumentNullException(nameof(texture));
        Border = border;
        SourceRect = sourceRect == default
            ? new IntRect(0, 0, (System.Int32)texture.Size.X, (System.Int32)texture.Size.Y)
            : sourceRect;

        // Init parts
        for (System.Int32 i = 0; i < 9; i++)
        {
            _parts[i] = new Sprite(Texture);
        }

        // Defaults
        Position = default;
        Size = new Vector2f(SourceRect.Width, SourceRect.Height);
        _dirty = true;
    }

    #region Fluent setters (auto-layout)
    public NineSlicePanel SetPosition(Vector2f pos)
    {
        if (pos != Position)
        {
            Position = pos;
            _dirty = true;
        }
        return this;
    }

    public NineSlicePanel SetSize(Vector2f size)
    {
        if (size != Size)
        {
            Size = size;
            _dirty = true;
        }
        return this;
    }

    public NineSlicePanel SetSourceRect(IntRect rect)
    {
        if (rect != SourceRect)
        {
            SourceRect = rect;
            _dirty = true;
        }
        return this;
    }

    public NineSlicePanel SetBorder(Thickness border)
    {
        if (!border.Equals(Border))
        {
            Border = border;
            _dirty = true;
        }
        return this;
    }

    public NineSlicePanel SetColor(Color color)
    {
        for (System.Int32 i = 0; i < _parts.Length; i++)
        {
            _parts[i].Color = color;
        }

        return this;
    }

    public Color GetColor() => _parts.Length > 0 ? _parts[0].Color : Color.White;

    /// <summary>For legacy code paths using your RenderObject style.</summary>
    public void Render(RenderTarget target)
    {
        EnsureLayout();
        for (System.Int32 i = 0; i < 9; i++)
        {
            target.Draw(_parts[i]);
        }
    }
    #endregion

    /// <summary>
    /// Recompute 9 slices geometry.
    /// </summary>
    public void Layout()
    {
        // Guard: avoid negative stretch if target too small
        System.Int32 L = Border.Left, T = Border.Top, R = Border.Right, B = Border.Bottom;
        System.Single minW = L + R;
        System.Single minH = T + B;

        System.Single x = (System.Single)System.Math.Round(Position.X);
        System.Single y = (System.Single)System.Math.Round(Position.Y);
        System.Single w = (System.Single)System.Math.Round(System.Math.Max(Size.X, minW));
        System.Single h = (System.Single)System.Math.Round(System.Math.Max(Size.Y, minH));

        System.Int32 sx = SourceRect.Left, sy = SourceRect.Top, sw = SourceRect.Width, sh = SourceRect.Height;

        // Source slices
        var src = new IntRect[9];
        // Corners
        src[0] = new IntRect(sx, sy, L, T);                           // TL
        src[2] = new IntRect(sx + sw - R, sy, R, T);                  // TR
        src[6] = new IntRect(sx, sy + sh - B, L, B);                  // BL
        src[8] = new IntRect(sx + sw - R, sy + sh - B, R, B);         // BR
        // Edges
        src[1] = new IntRect(sx + L, sy, sw - L - R, T);              // Top
        src[3] = new IntRect(sx, sy + T, L, sh - T - B);              // Left
        src[5] = new IntRect(sx + sw - R, sy + T, R, sh - T - B);     // Right
        src[7] = new IntRect(sx + L, sy + sh - B, sw - L - R, B);     // Bottom
        // Center
        src[4] = new IntRect(sx + L, sy + T, sw - L - R, sh - T - B); // Center

        // Target rects (draw borders at 1:1 for crisp)
        System.Single Lw = L, Tw = T, Rw = R, Bw = B;

        var dst = new FloatRect[9];
        // Corners
        dst[0] = new FloatRect(x, y, Lw, Tw);
        dst[2] = new FloatRect(x + w - Rw, y, Rw, Tw);
        dst[6] = new FloatRect(x, y + h - Bw, Lw, Bw);
        dst[8] = new FloatRect(x + w - Rw, y + h - Bw, Rw, Bw);
        // Edges
        dst[1] = new FloatRect(x + Lw, y, w - Lw - Rw, Tw);              // Top (stretch X)
        dst[3] = new FloatRect(x, y + Tw, Lw, h - Tw - Bw);              // Left (stretch Y)
        dst[5] = new FloatRect(x + w - Rw, y + Tw, Rw, h - Tw - Bw);     // Right (stretch Y)
        dst[7] = new FloatRect(x + Lw, y + h - Bw, w - Lw - Rw, Bw);     // Bottom (stretch X)
        // Center
        dst[4] = new FloatRect(x + Lw, y + Tw, w - Lw - Rw, h - Tw - Bw);

        // Apply
        for (System.Int32 i = 0; i < 9; i++)
        {
            var s = _parts[i];
            s.TextureRect = src[i];
            s.Position = new Vector2f(dst[i].Left, dst[i].Top);

            // Clamp to >= 0 to avoid NaN scales when very small
            System.Single dw = System.Math.Max(0f, dst[i].Width);
            System.Single dh = System.Math.Max(0f, dst[i].Height);
            System.Single swp = System.Math.Max(1, src[i].Width);
            System.Single shp = System.Math.Max(1, src[i].Height);

            s.Scale = new Vector2f(
                dw / swp,
                dh / shp
            );
        }

        _dirty = false;
    }

    private void EnsureLayout()
    {
        if (_dirty)
        {
            Layout();
        }
    }

    // SFML draw path
    public void Draw(RenderTarget target, RenderStates states)
    {
        EnsureLayout();
        for (System.Int32 i = 0; i < 9; i++)
        {
            target.Draw(_parts[i], states);
        }
    }
}

/// <summary>Simple thickness struct to describe 9-slice borders.</summary>
public readonly struct Thickness(System.Int32 left, System.Int32 top, System.Int32 right, System.Int32 bottom)
{
    public System.Int32 Left { get; } = left;

    public System.Int32 Top { get; } = top;

    public System.Int32 Right { get; } = right;

    public System.Int32 Bottom { get; } = bottom;

    public Thickness(System.Int32 uniform) : this(uniform, uniform, uniform, uniform) { }
}
