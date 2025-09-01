using Nalix.Rendering.Effects.Transitions.Abstractions;
using Nalix.Rendering.Effects.Transitions.Effects;
using Nalix.Rendering.Objects;
using Nalix.Rendering.Scenes;
using SFML.Graphics;

namespace Nalix.Rendering.Effects.Transitions;

/// <summary>
/// Runs a two-phase scene transition via a full-screen overlay:
/// <b>closing</b> (cover) → <b>switch scene</b> → <b>opening</b> (reveal).
/// </summary>
/// <remarks>
/// <para>
/// (VN) Hiệu ứng chuyển cảnh 2 pha. Nửa đầu <b>đóng</b> (che màn), đến giữa thì <b>đổi scene</b>,
/// nửa sau <b>mở</b> (trả lại màn). Vẽ bằng một strategy <see cref="ITransitionDrawable"/>.
/// </para>
/// <para>
/// The instance persists across scene changes and auto-destroys after finishing.
/// </para>
/// </remarks>
public sealed class SceneTransition : RenderObject
{
    #region ===== Fields (immutable configuration) =====

    private readonly System.Single _durationSeconds;
    private readonly System.String _nextSceneName;
    private readonly ITransitionDrawable _effect;

    #endregion

    #region ===== Runtime state =====

    private System.Single _elapsed;
    private System.Boolean _hasSwitched;

    #endregion

    #region ===== Construction =====

    /// <summary>
    /// Creates a new <see cref="SceneTransition"/>.
    /// </summary>
    /// <param name="nextScene">Target scene name to switch to at mid-point.</param>
    /// <param name="style">Transition visual style/strategy.</param>
    /// <param name="duration">Total duration in seconds (clamped to a minimum of 0.1s).</param>
    /// <param name="color">Overlay tint color (defaults to black).</param>
    /// <exception cref="ArgumentNullException"><paramref name="nextScene"/> is null.</exception>
    public SceneTransition(
        System.String nextScene,
        TransitionStyle style = TransitionStyle.Fade,
        System.Single duration = 1.0f,
        Color? color = null)
    {
        _nextSceneName = nextScene ?? throw new System.ArgumentNullException(nameof(nextScene));

        // (VN) Ép thời lượng tối thiểu để tránh chia 0 / blinking khó chịu
        _durationSeconds = System.Math.Max(0.1f, duration);

        Color overlay = color ?? Color.Black;

        // (VN) Chọn strategy vẽ overlay dựa theo style
        _effect = style switch
        {
            TransitionStyle.Fade => new FadeOverlay(overlay),
            TransitionStyle.WipeHorizontal => new WipeOverlayHorizontal(overlay),
            TransitionStyle.WipeVertical => new WipeOverlayVertical(overlay),
            TransitionStyle.SlideCoverLeft => new SlideCoverOverlay(overlay, fromLeft: true),
            TransitionStyle.SlideCoverRight => new SlideCoverOverlay(overlay, fromLeft: false),
            TransitionStyle.ZoomIn => new ZoomOverlay(overlay, modeIn: true),
            TransitionStyle.ZoomOut => new ZoomOverlay(overlay, modeIn: false),
            _ => new FadeOverlay(overlay),
        };

        // (VN) Luôn vẽ trên cùng và đi qua được scene change
        SetZIndex(System.Int32.MaxValue);
        PersistOnSceneChange = true;
    }

    #endregion

    #region ===== Engine hooks =====

    /// <summary>
    /// Advances the transition timer, updates overlay (close/open), switches scene at mid-point,
    /// and destroys itself upon completion.
    /// </summary>
    /// <param name="deltaTime">Elapsed time in seconds since last frame.</param>
    public override void Update(System.Single deltaTime)
    {
        // (VN) Tiến thời gian
        _elapsed += deltaTime;

        // (VN) Chia đôi: nửa đầu close, nửa sau open
        System.Single half = _durationSeconds * 0.5f;
        System.Boolean isClosing = _elapsed <= half;

        // (VN) Tiến độ cục bộ trong từng pha [0..1]
        System.Single localT = isClosing
            ? (_elapsed / half)
            : ((_elapsed - half) / half);

        // (VN) Kẹp phạm vi đề phòng tràn
        localT = System.Math.Clamp(localT, 0f, 1f);

        // (VN) Cập nhật strategy → nó sẽ tính toán Drawable nội bộ
        _effect.Update(localT, isClosing);

        // (VN) Đến nửa thời lượng thì chuyển scene (chỉ 1 lần)
        if (!_hasSwitched && _elapsed >= half)
        {
            _hasSwitched = true;
            SceneManager.ChangeScene(_nextSceneName);
        }

        // (VN) Kết thúc thì tự hủy
        if (_elapsed >= _durationSeconds)
        {
            Destroy();
        }
    }

    /// <summary>
    /// Provides the drawable for the current overlay frame.
    /// </summary>
    protected override Drawable GetDrawable() => _effect.GetDrawable();

    #endregion
}

/// <summary>
/// Convenience helpers for playing scene transitions.
/// </summary>
public static class SceneTransitions
{
    /// <summary>
    /// Creates, enqueues, and plays a scene transition effect.
    /// </summary>
    /// <param name="nextScene">Target scene name to switch to at mid-point.</param>
    /// <param name="style">Transition visual style/strategy.</param>
    /// <param name="duration">Total duration in seconds.</param>
    /// <param name="color">Overlay tint color (defaults to black).</param>
    /// <example>
    /// <code>
    /// // Fade to "battle" scene in 0.8s with black overlay
    /// SceneTransitions.Play("battle", TransitionStyle.Fade, 0.8f);
    /// </code>
    /// </example>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static void Play(
        System.String nextScene,
        TransitionStyle style = TransitionStyle.Fade,
        System.Single duration = 1.0f,
        Color? color = null)
        => new SceneTransition(nextScene, style, duration, color).Spawn();
}
