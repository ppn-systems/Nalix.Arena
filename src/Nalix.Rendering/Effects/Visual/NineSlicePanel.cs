using SFML.Graphics;
using SFML.System;

namespace Nalix.Rendering.Effects.Visual;

/// <summary>
/// Simple 9-slice panel for crisp, scalable UI frames.
/// Keep corners unscaled, edges scaled in one axis, center in both.
/// </summary>
public sealed class NineSlicePanel : Drawable
{
    public Vector2f Position { get; set; }
    public Vector2f Size { get; set; } // Target outer size in pixels
    public IntRect SourceRect { get; set; } // Sub-rect inside the texture, if needed
    public Thickness Border { get; }       // Border thickness in source pixels
    public Texture Texture { get; }

    private readonly Sprite[] _parts = new Sprite[9];

    /// <param name="texture">UI frame texture (set Smooth=false for pixel art)</param>
    /// <param name="border">Left/Top/Right/Bottom border thickness (in source pixels)</param>
    /// <param name="sourceRect">Optional source rect; pass IntRect() for full texture</param>
    public NineSlicePanel(Texture texture, Thickness border, IntRect sourceRect = default)
    {
        Texture = texture;
        Texture.Smooth = false; // crisp edges
        Border = border;
        SourceRect = sourceRect == default
            ? new IntRect(0, 0, (System.Int32)texture.Size.X, (System.Int32)texture.Size.Y)
            : sourceRect;

        for (System.Int32 i = 0; i < 9; i++)
        {
            _parts[i] = new Sprite(Texture);
        }
    }

    /// <summary>
    /// Recompute 9 slices geometry. Call after changing Position/Size.
    /// </summary>
    public void Layout()
    {
        // Source slices in texture space
        System.Int32 L = Border.Left, T = Border.Top, R = Border.Right, B = Border.Bottom;
        System.Int32 sx = SourceRect.Left, sy = SourceRect.Top, sw = SourceRect.Width, sh = SourceRect.Height;

        var src = new IntRect[9];
        // Corners
        src[0] = new IntRect(sx, sy, L, T);                             // TL
        src[2] = new IntRect(sx + sw - R, sy, R, T);                    // TR
        src[6] = new IntRect(sx, sy + sh - B, L, B);                    // BL
        src[8] = new IntRect(sx + sw - R, sy + sh - B, R, B);           // BR
        // Edges
        src[1] = new IntRect(sx + L, sy, sw - L - R, T);                // Top
        src[3] = new IntRect(sx, sy + T, L, sh - T - B);                // Left
        src[5] = new IntRect(sx + sw - R, sy + T, R, sh - T - B);       // Right
        src[7] = new IntRect(sx + L, sy + sh - B, sw - L - R, B);       // Bottom
        // Center
        src[4] = new IntRect(sx + L, sy + T, sw - L - R, sh - T - B);   // Center

        // Target rects in screen space (pixel-perfect integers recommended)
        System.Single x = (System.Single)System.Math.Round(Position.X);
        System.Single y = (System.Single)System.Math.Round(Position.Y);
        System.Single w = (System.Single)System.Math.Round(Size.X);
        System.Single h = (System.Single)System.Math.Round(Size.Y);

        System.Single Lw = L, Tw = T, Rw = R, Bw = B; // draw borders at 1:1 to keep crisp

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

        // Apply to sprites
        for (System.Int32 i = 0; i < 9; i++)
        {
            var s = _parts[i];
            s.TextureRect = src[i];
            s.Position = new Vector2f(dst[i].Left, dst[i].Top);
            // Scale from source size to destination size (scale per-axis)
            System.Single sxScale = src[i].Width == 0 ? 0f : dst[i].Width / src[i].Width;
            System.Single syScale = src[i].Height == 0 ? 0f : dst[i].Height / src[i].Height;
            s.Scale = new Vector2f(sxScale, syScale);
        }
    }

    // In NineSlicePanel
    public void SetColor(Color color)
    {
        for (System.Int32 i = 0; i < _parts.Length; i++)
        {
            _parts[i].Color = color;
        }
    }

    // Return color of the first part (others are the same)
    public Color GetColor() => _parts.Length > 0 ? _parts[0].Color : Color.White;

    public void Draw(RenderTarget target, RenderStates states)
    {
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

    public Thickness(System.Int32 uniform)
        : this(uniform, uniform, uniform, uniform) { }
}
