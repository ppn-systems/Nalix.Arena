using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Nalix.Mono.UI.Rendering;

public sealed class NineSliceTexture
{
    public Texture2D Texture { get; }

    public System.Int32 Left { get; }

    public System.Int32 Right { get; }

    public System.Int32 Top { get; }

    public System.Int32 Bottom { get; }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1510:Use ArgumentNullException throw helper", Justification = "<Pending>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0016:Use 'throw' expression", Justification = "<Pending>")]
    public NineSliceTexture(Texture2D texture, System.Int32 left, System.Int32 top, System.Int32 right, System.Int32 bottom)
    {
        if (texture == null)
        {
            throw new System.ArgumentNullException(nameof(texture));
        }
        this.Texture = texture;
        if (left < 0)
        {
            throw new System.ArgumentOutOfRangeException(nameof(left), "Border thickness cannot be negative.");
        }
        if (top < 0)
        {
            throw new System.ArgumentOutOfRangeException(nameof(top), "Border thickness cannot be negative.");
        }
        if (right < 0)
        {
            throw new System.ArgumentOutOfRangeException(nameof(right), "Border thickness cannot be negative.");
        }
        if (bottom < 0)
        {
            throw new System.ArgumentOutOfRangeException(nameof(bottom), "Border thickness cannot be negative.");
        }
        if (left + right > texture.Width || top + bottom > texture.Height)
        {
            throw new System.ArgumentException("Sum of borders exceeds texture size.");
        }
        this.Top = top;
        this.Left = left;
        this.Right = right;
        this.Bottom = bottom;
    }

    public NineSliceTexture(Texture2D texture)
        : this(texture, 16, 16, 16, 16)
    {
    }

    public void Draw(SpriteBatch spriteBatch, Rectangle destination, Color color)
    {
        System.ArgumentNullException.ThrowIfNull(spriteBatch, nameof(spriteBatch));
        System.Int32 width = this.Texture.Width;
        System.Int32 texHeight = this.Texture.Height;
        System.Int32 centerSrcWidth = width - this.Left - this.Right;
        System.Int32 centerSrcHeight = texHeight - this.Top - this.Bottom;
        System.Int32 leftWidth = this.Left;
        System.Int32 rightWidth = this.Right;
        System.Int32 topHeight = this.Top;
        System.Int32 bottomHeight = this.Bottom;
        System.Int32 centerDestWidth = System.Math.Max(0, destination.Width - leftWidth - rightWidth);
        System.Int32 centerDestHeight = System.Math.Max(0, destination.Height - topHeight - bottomHeight);
        this.DrawPart(spriteBatch, new Rectangle(0, 0, this.Left, this.Top), new Rectangle(destination.X, destination.Y, this.Left, this.Top), color);
        this.DrawPart(spriteBatch, new Rectangle(this.Left, 0, centerSrcWidth, this.Top), new Rectangle(destination.X + this.Left, destination.Y, centerDestWidth, this.Top), color);
        this.DrawPart(spriteBatch, new Rectangle(this.Left + centerSrcWidth, 0, this.Right, this.Top), new Rectangle(destination.Right - this.Right, destination.Y, this.Right, this.Top), color);
        this.DrawPart(spriteBatch, new Rectangle(0, this.Top, this.Left, centerSrcHeight), new Rectangle(destination.X, destination.Y + this.Top, this.Left, centerDestHeight), color);
        this.DrawPart(spriteBatch, new Rectangle(this.Left, this.Top, centerSrcWidth, centerSrcHeight), new Rectangle(destination.X + this.Left, destination.Y + this.Top, centerDestWidth, centerDestHeight), color);
        this.DrawPart(spriteBatch, new Rectangle(this.Left + centerSrcWidth, this.Top, this.Right, centerSrcHeight), new Rectangle(destination.Right - this.Right, destination.Y + this.Top, this.Right, centerDestHeight), color);
        this.DrawPart(spriteBatch, new Rectangle(0, this.Top + centerSrcHeight, this.Left, this.Bottom), new Rectangle(destination.X, destination.Bottom - this.Bottom, this.Left, this.Bottom), color);
        this.DrawPart(spriteBatch, new Rectangle(this.Left, this.Top + centerSrcHeight, centerSrcWidth, this.Bottom), new Rectangle(destination.X + this.Left, destination.Bottom - this.Bottom, centerDestWidth, this.Bottom), color);
        this.DrawPart(spriteBatch, new Rectangle(this.Left + centerSrcWidth, this.Top + centerSrcHeight, this.Right, this.Bottom), new Rectangle(destination.Right - this.Right, destination.Bottom - this.Bottom, this.Right, this.Bottom), color);
    }

    private void DrawPart(SpriteBatch spriteBatch, Rectangle source, Rectangle destination, Color color)
    {
        if (destination.Width <= 0 || destination.Height <= 0)
        {
            return;
        }
        spriteBatch.Draw(this.Texture, destination, new Rectangle?(source), color);
    }
}
