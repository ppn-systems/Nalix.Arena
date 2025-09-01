namespace Nalix.Rendering.Physics;

/// <summary>
/// Represents an <b>Axis-Aligned Bounding Box</b> (AABB).
/// </summary>
/// <remarks>
/// <para>
/// An AABB is a rectangle aligned with the X and Y axes, defined by its minimum and maximum coordinates.
/// </para>
/// <para>
/// (VN) Hộp bao quanh thẳng trục: định nghĩa bởi <c>MinX, MinY</c> và <c>MaxX, MaxY</c>.
/// Dùng để kiểm tra va chạm nhanh trong 2D.
/// </para>
/// </remarks>
public readonly struct AABB
{
    #region ===== Fields =====

    /// <summary>Minimum X coordinate (left edge).</summary>
    public readonly System.Single MinX;

    /// <summary>Minimum Y coordinate (top edge).</summary>
    public readonly System.Single MinY;

    /// <summary>Maximum X coordinate (right edge).</summary>
    public readonly System.Single MaxX;

    /// <summary>Maximum Y coordinate (bottom edge).</summary>
    public readonly System.Single MaxY;

    #endregion

    #region ===== Construction =====

    /// <summary>
    /// Creates a new <see cref="AABB"/> given min and max coordinates.
    /// </summary>
    /// <param name="minX">Minimum X (left).</param>
    /// <param name="minY">Minimum Y (top).</param>
    /// <param name="maxX">Maximum X (right).</param>
    /// <param name="maxY">Maximum Y (bottom).</param>
    public AABB(System.Single minX, System.Single minY, System.Single maxX, System.Single maxY)
    {
        MinX = minX;
        MinY = minY;
        MaxX = maxX;
        MaxY = maxY;
    }

    /// <summary>
    /// Creates a new <see cref="AABB"/> from a starting corner (left,top) and size (width,height).
    /// </summary>
    /// <param name="left">Left (minimum X).</param>
    /// <param name="top">Top (minimum Y).</param>
    /// <param name="width">Width in units.</param>
    /// <param name="height">Height in units.</param>
    /// <remarks>(VN) Factory method: dễ dùng khi có gốc trái + kích thước.</remarks>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static AABB FromMinSize(System.Single left, System.Single top, System.Single width, System.Single height)
        => new(left, top, left + width, top + height);

    #endregion

    #region ===== Properties =====

    /// <summary>Width of the box.</summary>
    public System.Single Width => MaxX - MinX;

    /// <summary>Height of the box.</summary>
    public System.Single Height => MaxY - MinY;

    /// <summary>Center point of the box.</summary>
    public (System.Single X, System.Single Y) Center => ((MinX + MaxX) * 0.5f, (MinY + MaxY) * 0.5f);

    #endregion

    #region ===== Methods =====

    /// <summary>
    /// Tests if this box intersects with another.
    /// </summary>
    /// <param name="other">Other AABB.</param>
    /// <returns><c>true</c> if overlapping; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// (VN) Intersects = không bị tách rời trên cả trục X và trục Y.
    /// </remarks>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public System.Boolean Intersects(in AABB other)
        => !(other.MinX > MaxX || other.MaxX < MinX ||
             other.MinY > MaxY || other.MaxY < MinY);

    /// <inheritdoc/>
    public override System.String ToString()
        => $"AABB(MinX={MinX}, MinY={MinY}, MaxX={MaxX}, MaxY={MaxY})";

    #endregion
}
