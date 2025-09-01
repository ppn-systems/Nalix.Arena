using Nalix.Rendering.Effects.Transitions.Abstractions;
using Nalix.Rendering.Effects.Transitions.Effects;
using Nalix.Rendering.Objects;
using Nalix.Rendering.Scenes;
using SFML.Graphics;
using System;

namespace Nalix.Rendering.Effects.Transitions;

/// <summary>
/// Hiệu ứng chuyển cảnh đa dạng bằng overlay 2 pha: che (close) -> đổi scene -> mở (open).
/// </summary>
public sealed class SceneTransition : RenderObject
{
    private readonly Single _duration;
    private readonly String _nextScene;
    private readonly ITransitionDrawable _effect;

    private Single _t;
    private Boolean _switched;

    /// <param name="nextScene">Tên scene chuyển đến.</param>
    /// <param name="style">Kiểu hiệu ứng.</param>
    /// <param name="duration">Tổng thời gian (giây).</param>
    /// <param name="color">Màu phủ (mặc định đen).</param>
    public SceneTransition(String nextScene,
                           TransitionStyle style = TransitionStyle.Fade,
                           Single duration = 1.0f,
                           Color? color = null)
    {
        _nextScene = nextScene ?? throw new ArgumentNullException(nameof(nextScene));
        _duration = Math.Max(0.1f, duration);
        var c = color ?? Color.Black;

        // Tạo strategy hiệu ứng
        _effect = style switch
        {
            TransitionStyle.Fade => new FadeOverlay(c),
            TransitionStyle.WipeHorizontal => new WipeOverlayHorizontal(c),
            TransitionStyle.WipeVertical => new WipeOverlayVertical(c),
            TransitionStyle.SlideCoverLeft => new SlideCoverOverlay(c, fromLeft: true),
            TransitionStyle.SlideCoverRight => new SlideCoverOverlay(c, fromLeft: false),
            TransitionStyle.ZoomIn => new ZoomOverlay(c, modeIn: true),
            TransitionStyle.ZoomOut => new ZoomOverlay(c, modeIn: false),
            _ => new FadeOverlay(c),
        };

        // đứng trên cùng và tồn tại xuyên scene-change
        SetZIndex(Int32.MaxValue);
        PersistOnSceneChange = true;
    }

    public override void Update(Single dt)
    {
        _t += dt;
        Single half = _duration * 0.5f;

        Boolean closing = _t <= half;
        Single local = closing ? (_t / half) : ((_t - half) / half);
        local = Math.Clamp(local, 0f, 1f);

        _effect.Update(local, closing);

        if (!_switched && _t >= half)
        {
            _switched = true;
            SceneManager.ChangeScene(_nextScene);
        }

        if (_t >= _duration)
        {
            Destroy();
        }
    }

    protected override Drawable GetDrawable() => _effect.GetDrawable();
}

/// <summary>
/// Helper gọn để gọi chuyển cảnh.
/// </summary>
public static class SceneTransitions
{
    /// <summary>
    /// Tạo và chạy hiệu ứng chuyển cảnh.
    /// </summary>
    public static void Play(String nextScene,
                           TransitionStyle style = TransitionStyle.Fade,
                           Single duration = 1.0f,
                           Color? color = null)
        => new SceneTransition(nextScene, style, duration, color).Spawn();
}
