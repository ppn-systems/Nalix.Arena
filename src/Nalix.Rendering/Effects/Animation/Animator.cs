using SFML.Graphics;

namespace Nalix.Rendering.Effects.Animation;

/// <summary>
/// Lightweight frame animator for a <see cref="Sprite"/> using a list of <see cref="IntRect"/> frames.
/// </summary>
/// <remarks>
/// <para>
/// (VN) Quản lý hoạt ảnh theo khung hình (spritesheet). Mỗi khung là 1 <c>IntRect</c> cắt từ <c>Texture</c>.
/// Gọi <see cref="Update(System.Single)"/> mỗi frame để tiến thời gian; khi đủ <see cref="FrameTime"/> sẽ nhảy sang khung kế.
/// </para>
/// <para>
/// Mặc định: <see cref="Loop"/> = <c>true</c>, <see cref="Playing"/> = <c>false</c> cho tới khi gọi <see cref="Play"/>.
/// </para>
/// </remarks>
public sealed class Animator
{
    #region ===== Fields =====

    private readonly System.Collections.Generic.List<IntRect> _frames = [];
    private readonly Sprite _sprite;

    private System.Single _frameTime;     // seconds per frame
    private System.Single _accumulator;   // accumulated time since last advance
    private System.Int32 _index;          // current frame index

    #endregion

    #region ===== Construction =====

    /// <summary>
    /// Creates a new animator bound to a <see cref="Sprite"/>.
    /// </summary>
    /// <param name="sprite">Target sprite whose <see cref="Sprite.TextureRect"/> will be updated.</param>
    /// <param name="frameTime">Seconds per frame (min 0.001s).</param>
    /// <exception cref="System.ArgumentNullException"><paramref name="sprite"/> is null.</exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "<Pending>")]
    public Animator(Sprite sprite, System.Single frameTime = 0.1f)
    {
        _sprite = sprite ?? throw new System.ArgumentNullException(nameof(sprite));
        _frameTime = System.Math.Max(0.001f, frameTime);
    }

    #endregion

    #region ===== Properties =====

    /// <summary>Whether the animation should loop when reaching the end.</summary>
    public System.Boolean Loop { get; set; } = true;

    /// <summary>Whether the animation is currently advancing frames.</summary>
    public System.Boolean Playing { get; private set; }

    /// <summary>Total number of frames.</summary>
    public System.Int32 FrameCount => _frames.Count;

    /// <summary>Current frame index (0-based). Returns -1 if there is no frame.</summary>
    public System.Int32 CurrentFrameIndex => _frames.Count == 0 ? -1 : _index;

    /// <summary>True if no frames are set.</summary>
    public System.Boolean IsEmpty => _frames.Count == 0;

    /// <summary>
    /// Seconds per frame. Clamped to a minimum of 0.001s.
    /// </summary>
    public System.Single FrameTime
    {
        get => _frameTime;
        set => _frameTime = System.Math.Max(0.001f, value);
    }

    #endregion

    #region ===== Frame Management =====

    /// <summary>
    /// Replaces all frames with the given sequence and resets to the first frame.
    /// </summary>
    public void SetFrames(System.Collections.Generic.IEnumerable<IntRect> frames)
    {
        _frames.Clear();
        if (frames != null)
        {
            _frames.AddRange(frames);
        }
        ResetHead();
        ApplyFrame();
    }

    /// <summary>
    /// Adds a single frame to the end of the list.
    /// </summary>
    public void AddFrame(IntRect frame) => _frames.Add(frame);

    /// <summary>
    /// Adds multiple frames to the end of the list.
    /// </summary>
    public void AddFrames(System.Collections.Generic.IEnumerable<IntRect> frames)
    {
        if (frames != null)
        {
            _frames.AddRange(frames);
        }
    }

    /// <summary>
    /// Clears all frames and stops the animation.
    /// </summary>
    public void ClearFrames()
    {
        _frames.Clear();
        Stop(); // also resets head & apply (no-op when empty)
    }

    /// <summary>
    /// Jump to a specific frame index and apply it immediately.
    /// </summary>
    /// <param name="index">0-based index.</param>
    public void GotoFrame(System.Int32 index)
    {
        if (_frames.Count == 0)
        {
            _index = 0;
            return;
        }

        _index = System.Math.Clamp(index, 0, _frames.Count - 1);
        _accumulator = 0f;
        ApplyFrame();
    }

    /// <summary>
    /// Helper to build frames from a grid (rows x cols) on a spritesheet.
    /// </summary>
    /// <param name="cellWidth">Width of each cell in pixels.</param>
    /// <param name="cellHeight">Height of each cell in pixels.</param>
    /// <param name="columns">Number of columns.</param>
    /// <param name="rows">Number of rows.</param>
    /// <param name="startCol">Start column (0-based).</param>
    /// <param name="startRow">Start row (0-based).</param>
    /// <param name="count">How many frames to take (scan row-major). If null, uses all remaining.</param>
    /// <remarks>(VN) Duyệt theo hàng → cột. Hữu ích khi spritesheet chia ô đều nhau.</remarks>
    public void BuildGridFrames(
        System.Int32 cellWidth,
        System.Int32 cellHeight,
        System.Int32 columns,
        System.Int32 rows,
        System.Int32 startCol = 0,
        System.Int32 startRow = 0,
        System.Int32? count = null)
    {
        var list = new System.Collections.Generic.List<IntRect>();
        System.Int32 total = columns * rows;
        System.Int32 start = (startRow * columns) + startCol;
        System.Int32 take = count ?? (total - start);

        for (System.Int32 k = 0; k < take; k++)
        {
            System.Int32 id = start + k;
            if (id >= total)
            {
                break;
            }

            System.Int32 r = id / columns;
            System.Int32 c = id % columns;

            list.Add(new IntRect(c * cellWidth, r * cellHeight, cellWidth, cellHeight));
        }

        SetFrames(list);
    }

    #endregion

    #region ===== Playback Control =====

    /// <summary>Start advancing frames.</summary>
    public void Play() => Playing = true;

    /// <summary>Pause advancing frames (keeps current frame).</summary>
    public void Pause() => Playing = false;

    /// <summary>Stop and reset to the first frame.</summary>
    public void Stop()
    {
        Playing = false;
        ResetHead();
        ApplyFrame();
    }

    /// <summary>
    /// Set seconds per frame (alias of <see cref="FrameTime"/> setter).
    /// </summary>
    public void SetFrameTime(System.Single seconds) => FrameTime = seconds;

    #endregion

    #region ===== Update Loop =====

    /// <summary>
    /// Advance the animation by <paramref name="deltaTime"/> seconds.
    /// </summary>
    /// <param name="deltaTime">Elapsed seconds since last call.</param>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public void Update(System.Single deltaTime)
    {
        if (!Playing || _frames.Count == 0)
        {
            return;
        }

        _accumulator += deltaTime;

        // (VN) Có thể “bù” nhiều frame nếu dt lớn (lag spike)
        while (_accumulator >= _frameTime)
        {
            _accumulator -= _frameTime;
            System.Int32 next = _index + 1;

            if (next >= _frames.Count)
            {
                if (Loop)
                {
                    next = 0;
                    OnLooped?.Invoke();
                }
                else
                {
                    // (VN) Dừng ở frame cuối và phát sự kiện hoàn tất
                    _index = _frames.Count - 1;
                    ApplyFrame();
                    Playing = false;
                    OnCompleted?.Invoke();
                    break;
                }
            }

            _index = next;
            ApplyFrame();
        }
    }

    #endregion

    #region ===== Events =====

    /// <summary>
    /// Raised when a non-looping animation reaches the end and stops.
    /// </summary>
    public event System.Action OnCompleted;

    /// <summary>
    /// Raised when a looping animation wraps from the last frame back to the first.
    /// </summary>
    public event System.Action OnLooped;

    #endregion

    #region ===== Internals =====

    private void ResetHead()
    {
        _index = 0;
        _accumulator = 0f;
    }

    private void ApplyFrame()
    {
        if (_frames.Count == 0)
        {
            return;
        }

        // (VN) Cập nhật khung lên Sprite
        _sprite.TextureRect = _frames[_index];
    }

    #endregion
}
