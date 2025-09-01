using Nalix.Rendering.Objects;
using SFML.Graphics;
using SFML.System;

namespace Nalix.Rendering.Effects.Animation;

/// <summary>
/// Base class for sprite-based scene objects that have a built-in <see cref="Animator"/>.
/// </summary>
/// <remarks>
/// <para>
/// (VN) Lớp nền cho mọi object dùng <see cref="Sprite"/> và có hoạt ảnh khung hình (spritesheet).
/// Mặc định, <see cref="Update(System.Single)"/> sẽ gọi <see cref="Animator.Update(System.Single)"/> để tiến khung.
/// </para>
/// <para>
/// Dùng <see cref="SetAnimationFrames(System.Collections.Generic.IEnumerable{IntRect}, System.Single, System.Boolean)"/>
/// hoặc các overload tiện dụng để khởi tạo khung và bắt đầu chạy animation.
/// </para>
/// </remarks>
public abstract class AnimatedSpriteObject : SpriteObject
{
    /// <summary>
    /// The frame animator bound to the underlying <see cref="SpriteObject.Sprite"/>.
    /// </summary>
    protected readonly Animator Animator;

    #region ===== Constructors (mirror SpriteObject) =====

    /// <inheritdoc/>
    protected AnimatedSpriteObject(Texture texture)
        : base(texture) => Animator = new Animator(Sprite);

    /// <inheritdoc/>
    protected AnimatedSpriteObject(Texture texture, IntRect rect)
        : base(texture, rect) => Animator = new Animator(Sprite);

    /// <inheritdoc/>
    protected AnimatedSpriteObject(Texture texture, Vector2f position, Vector2f scale, System.Single rotation)
        : base(texture, position, scale, rotation) => Animator = new Animator(Sprite);

    /// <inheritdoc/>
    protected AnimatedSpriteObject(Texture texture, IntRect rect, Vector2f position, Vector2f scale, System.Single rotation)
        : base(texture, rect, position, scale, rotation) => Animator = new Animator(Sprite);

    #endregion

    #region ===== Public API (helpers) =====

    /// <summary>
    /// Sets frames, frame time, loop flag and starts playing immediately.
    /// </summary>
    /// <param name="frames">Sequence of sprite-rect frames (in draw order).</param>
    /// <param name="frameTime">Seconds per frame.</param>
    /// <param name="loop">Whether the animation should loop.</param>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public void SetAnimationFrames(
        System.Collections.Generic.IEnumerable<IntRect> frames,
        System.Single frameTime,
        System.Boolean loop = true)
    {
        Animator.SetFrames(frames);
        Animator.SetFrameTime(frameTime);
        Animator.Loop = loop;
        Animator.Play();
    }

    /// <summary>
    /// Convenience overload: builds frames from a grid (row-major) and starts playing.
    /// </summary>
    /// <param name="cellWidth">Cell width in pixels.</param>
    /// <param name="cellHeight">Cell height in pixels.</param>
    /// <param name="columns">Number of columns.</param>
    /// <param name="rows">Number of rows.</param>
    /// <param name="frameTime">Seconds per frame.</param>
    /// <param name="loop">Whether the animation should loop.</param>
    /// <param name="startCol">Start column (0-based).</param>
    /// <param name="startRow">Start row (0-based).</param>
    /// <param name="count">How many frames to take (null = all remaining).</param>
    /// <remarks>(VN) Dùng khi spritesheet chia ô đều nhau.</remarks>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public void SetAnimationFromGrid(
        System.Int32 cellWidth, System.Int32 cellHeight,
        System.Int32 columns, System.Int32 rows,
        System.Single frameTime,
        System.Boolean loop = true,
        System.Int32 startCol = 0, System.Int32 startRow = 0,
        System.Int32? count = null)
    {
        Animator.BuildGridFrames(cellWidth, cellHeight, columns, rows, startCol, startRow, count);
        Animator.SetFrameTime(frameTime);
        Animator.Loop = loop;
        Animator.Play();
    }

    /// <summary>
    /// Starts (or resumes) the current animation.
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public void Play() => Animator.Play();

    /// <summary>
    /// Pauses the current animation (keeps current frame).
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public new void Pause() => Animator.Pause();

    /// <summary>
    /// Stops the animation and resets to the first frame.
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public void Stop() => Animator.Stop();

    #endregion

    #region ===== Engine hook =====

    /// <summary>
    /// Advances the bound <see cref="Animator"/> by <paramref name="deltaTime"/>.
    /// </summary>
    /// <param name="deltaTime">Elapsed seconds since last update.</param>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public override void Update(System.Single deltaTime)
        => Animator.Update(deltaTime);

    #endregion

    #region ===== Optional event hooks (override in derived classes) =====

    // (VN) Nếu bạn muốn xử lý khi loop/complete, có thể đăng ký ở ctor con:
    // Animator.OnLooped += () => OnAnimationLooped();
    // Animator.OnCompleted += () => OnAnimationCompleted();

    /// <summary>
    /// Called when a looping animation wraps from last frame to first.
    /// (VN) Override trong lớp con nếu cần.
    /// </summary>
    protected virtual void OnAnimationLooped() { }

    /// <summary>
    /// Called when a non-looping animation reaches its end and stops.
    /// (VN) Override trong lớp con nếu cần.
    /// </summary>
    protected virtual void OnAnimationCompleted() { }

    #endregion
}
