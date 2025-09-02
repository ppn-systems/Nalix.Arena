namespace Nalix.Rendering.Effects.Visual;

/// <summary>
/// Simple thickness struct to describe 9-slice borders.
/// </summary>
public readonly struct Thickness(System.Int32 left, System.Int32 top, System.Int32 right, System.Int32 bottom)
{
    public System.Int32 Left { get; } = left;

    public System.Int32 Top { get; } = top;

    public System.Int32 Right { get; } = right;

    public System.Int32 Bottom { get; } = bottom;

    public Thickness(System.Int32 uniform) : this(uniform, uniform, uniform, uniform) { }
}

