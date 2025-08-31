namespace Nalix.Rendering.Physics;

/// <summary>Axis-Aligned Bounding Box.</summary>
public readonly struct AABB(System.Single minX, System.Single minY, System.Single maxX, System.Single maxY)
{
    public readonly System.Single MinX = minX, MinY = minY;
    public readonly System.Single MaxX = maxX, MaxY = maxY;

    public System.Single Width => MaxX - MinX;
    public System.Single Height => MaxY - MinY;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public System.Boolean Intersects(in AABB other)
        => !(other.MinX > MaxX || other.MaxX < MinX || other.MinY > MaxY || other.MaxY < MinY);

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static AABB FromMinSize(System.Single left, System.Single top, System.Single width, System.Single height)
        => new(left, top, left + width, top + height);

    public override System.String ToString() => $"AABB({MinX},{MinY},{MaxX},{MaxY})";
}
