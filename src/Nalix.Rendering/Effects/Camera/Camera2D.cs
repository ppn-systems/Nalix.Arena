using SFML.Graphics;
using SFML.System;

namespace Nalix.Rendering.Effects.Camera;

/// <summary>
/// Simple 2D camera with smooth follow, zoom and screen shake.
/// </summary>
/// <remarks>
/// <para>
/// (VN) Camera 2D: bám theo mục tiêu (lerp), phóng to/thu nhỏ (zoom) và rung màn (shake).
/// Gọi <see cref="Update(System.Single)"/> mỗi khung, rồi <see cref="Apply(RenderTarget)"/> trước khi vẽ.
/// </para>
/// </remarks>
public static class Camera2D
{
    #region ===== Fields & State =====

    private static Vector2f _targetPos;
    private static System.Single _shakeTime, _shakeStrength;

    private static System.Boolean _hasBounds;
    private static Vector2f _boundsMin, _boundsMax;

    /// <summary>
    /// Units per second for the follow-lerp. Larger = nhanh hơn.
    /// </summary>
    public static System.Single LerpSpeed { get; set; } = 10f;

    /// <summary>
    /// The active SFML view used by the camera.
    /// </summary>
    public static View Current { get; private set; }

    #endregion

    #region ===== Initialization & Reset =====

    /// <summary>
    /// Initializes the camera with a screen-sized view.
    /// </summary>
    /// <param name="screenSize">Window size in pixels (width,height).</param>
    public static void Initialize(Vector2u screenSize)
    {
        Current = new View(new FloatRect(0, 0, screenSize.X, screenSize.Y));
        _targetPos = Current.Center;
        Zoom = 1f;
        _shakeTime = _shakeStrength = 0f;
        _hasBounds = false;
    }

    /// <summary>
    /// Resets camera center/zoom and clears shake/bounds.
    /// </summary>
    /// <param name="center">New center in world coord.</param>
    /// <param name="zoom">Zoom factor &gt; 0 (1 = no zoom).</param>
    public static void Reset(Vector2f center, System.Single zoom = 1f)
    {
        _targetPos = center;
        Current.Center = center;
        _hasBounds = false;
        SetZoom(zoom);
        _shakeTime = _shakeStrength = 0f;
    }

    #endregion

    #region ===== Follow / Movement =====

    /// <summary>
    /// Sets the target position for smooth following.
    /// </summary>
    public static void Follow(Vector2f worldPos) => _targetPos = worldPos;

    /// <summary>
    /// Nudges camera target by a delta (VN: “đẩy” mục tiêu 1 đoạn).
    /// </summary>
    public static void Nudge(Vector2f delta) => _targetPos += delta;

    /// <summary>
    /// Directly sets camera center (no smoothing).
    /// </summary>
    public static void SetCenter(Vector2f center)
    {
        _targetPos = center;
        Current.Center = center;
        ClampToBounds();
    }

    /// <summary>
    /// Gets/sets camera center (world coords).
    /// </summary>
    public static Vector2f Center
    {
        get => Current.Center;
        set => SetCenter(value);
    }

    #endregion

    #region ===== Zoom =====

    /// <summary>
    /// Sets absolute zoom factor (&gt; 0). 1 = default scale.
    /// </summary>
    public static void SetZoom(System.Single factor)
    {
        if (factor <= 0f)
        {
            return;
        }

        System.Single scale = factor / Zoom;   // apply relative delta
        Current.Zoom(scale);
        Zoom = factor;

        // (VN) Zoom có thể làm “lộ” viền → kẹp lại theo bounds nếu có
        ClampToBounds();
    }

    /// <summary>
    /// Multiplies current zoom by <paramref name="factor"/>.
    /// </summary>
    public static void ZoomBy(System.Single factor) => SetZoom(Zoom * factor);

    /// <summary>
    /// Current absolute zoom factor.
    /// </summary>
    public static System.Single Zoom { get; private set; } = 1f;

    /// <summary>
    /// Current view size in world units (affected by zoom).
    /// </summary>
    public static Vector2f ViewportSizeWorld => Current.Size;

    #endregion

    #region ===== Bounds (optional) =====

    /// <summary>
    /// Constrains the camera so that the view rectangle stays inside the given world rectangle.
    /// </summary>
    /// <param name="worldRect">World-space rectangle (left, top, width, height).</param>
    /// <remarks>
    /// (VN) Camera center sẽ bị kẹp để không “nhìn” ra ngoài biên.
    /// </remarks>
    public static void SetBounds(FloatRect worldRect)
    {
        _hasBounds = true;
        _boundsMin = new Vector2f(worldRect.Left, worldRect.Top);
        _boundsMax = new Vector2f(worldRect.Left + worldRect.Width, worldRect.Top + worldRect.Height);
        ClampToBounds();
    }

    /// <summary>
    /// Clears any previously set bounds constraint.
    /// </summary>
    public static void ClearBounds() => _hasBounds = false;

    private static void ClampToBounds()
    {
        if (!_hasBounds)
        {
            return;
        }

        // View.Size is in world-units and already reflects zoom.
        System.Single halfW = Current.Size.X * 0.5f;
        System.Single halfH = Current.Size.Y * 0.5f;

        // (VN) Nếu bounds nhỏ hơn viewport → kẹp center vào chính giữa bounds
        System.Single minX = _boundsMin.X + halfW;
        System.Single maxX = _boundsMax.X - halfW;
        System.Single minY = _boundsMin.Y + halfH;
        System.Single maxY = _boundsMax.Y - halfH;

        Vector2f c = Current.Center;

        // Khi max < min (viewport lớn hơn bounds), ta “ghim” vào tâm bounds
        c.X = maxX < minX ? (_boundsMin.X + _boundsMax.X) * 0.5f : System.Math.Clamp(c.X, minX, maxX);

        c.Y = maxY < minY ? (_boundsMin.Y + _boundsMax.Y) * 0.5f : System.Math.Clamp(c.Y, minY, maxY);

        Current.Center = c;
    }

    #endregion

    #region ===== Shake =====

    /// <summary>
    /// Adds a simple screen shake with given <paramref name="strength"/> and duration in seconds.
    /// </summary>
    public static void Shake(System.Single strength, System.Single time)
    {
        _shakeStrength = System.MathF.Max(0f, strength);
        _shakeTime = System.MathF.Max(0f, time);
    }

    #endregion

    #region ===== Update & Apply =====

    /// <summary>
    /// Updates smooth follow and shake. Call once per frame.
    /// </summary>
    /// <param name="deltaTime">Seconds since last frame.</param>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static void Update(System.Single deltaTime)
    {
        // Smooth follow (VN: lerp theo tốc độ cấu hình)
        var c = Current.Center;
        System.Single k = System.MathF.Min(1f, deltaTime * System.MathF.Max(0f, LerpSpeed));
        c += (_targetPos - c) * k;

        // Shake (random offset mỗi frame)
        if (_shakeTime > 0f)
        {
            _shakeTime -= deltaTime;
            System.Single ox = (System.Random.Shared.NextSingle() - 0.5f) * 2f * _shakeStrength;
            System.Single oy = (System.Random.Shared.NextSingle() - 0.5f) * 2f * _shakeStrength;
            Current.Center = new Vector2f(c.X + ox, c.Y + oy);
        }
        else
        {
            Current.Center = c;
        }

        // Kẹp vào bounds nếu có
        ClampToBounds();
    }

    /// <summary>
    /// Applies the current view to the given render target.
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static void Apply(RenderTarget target) => target.SetView(Current);

    #endregion

    #region ===== Coordinate Helpers =====

    /// <summary>
    /// Converts world coordinates to screen pixels for the given window.
    /// </summary>
    public static Vector2i WorldToScreen(RenderWindow window, Vector2f world)
        => window.MapCoordsToPixel(world, Current);

    /// <summary>
    /// Converts screen pixels to world coordinates for the given window.
    /// </summary>
    public static Vector2f ScreenToWorld(RenderWindow window, Vector2i pixel)
        => window.MapPixelToCoords(pixel, Current);

    #endregion
}
